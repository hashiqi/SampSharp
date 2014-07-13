#include "SampSharp.h"

#include "TimeUtil.h"
#include "MonoUtil.h"

#include <iostream>
#include <fstream>
#include <mono/metadata/threads.h>
#include <mono/metadata/exception.h>
#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/debug-helpers.h>
#include <sampgdk/interop.h>
#include <assert.h>
#include "PathUtil.h"
#include "Natives.h"

using namespace std;

MonoDomain *SampSharp::root;
GamemodeImage SampSharp::gamemode;
GamemodeImage SampSharp::basemode;

uint32_t SampSharp::gameModeHandle;
EventMap SampSharp::events;

void SampSharp::Load(const char *basemode_path, const char *gamemode_path,
                     const char *gamemode_namespace,
                     const char *gamemode_class, bool debug) {

	#ifdef _WIN32
	mono_set_dirs(PathUtil::GetLibDirectory().c_str(), 
        PathUtil::GetConfigDirectory().c_str());
	#endif

	mono_debug_init(MONO_DEBUG_FORMAT_MONO);

	root = mono_jit_init(PathUtil::GetPathInBin(gamemode_path).c_str());

	#ifdef _WIN32
	if (debug == true) {
		sampgdk::logprintf("[SampSharp] Generating symbol files");
		MonoUtil::GenerateSymbols(basemode_path);
		MonoUtil::GenerateSymbols(gamemode_path);
	}
	#endif

    gamemode.image = mono_assembly_get_image(mono_domain_assembly_open(root, PathUtil::GetPathInBin(gamemode_path).c_str()));
	basemode.image = mono_assembly_get_image(mono_domain_assembly_open(root, PathUtil::GetPathInBin(basemode_path).c_str()));
    
	basemode.klass = mono_class_from_name(basemode.image, BASEMODE_NAMESPACE, BASEMODE_CLASS);
	gamemode.klass = mono_class_from_name(gamemode.image, gamemode_namespace, gamemode_class);

	LoadNatives(); 

	MonoObject *gamemode_obj = mono_object_new(mono_domain_get(), gamemode.klass);
	gameModeHandle = mono_gchandle_new(gamemode_obj, true);
	mono_runtime_object_init(gamemode_obj);
}

void SAMPGDK_CALL SampSharp::ProcessTimerTick(int timerid, void *data) {
    static MonoMethod *method;

    if (method == NULL) {
        method = LoadEvent("OnTimerTick", 2);
    }

	void *args[2];
	args[0] = &timerid;
	args[1] = data;
	CallEvent(method, args);
}

void SampSharp::ProcessTick() {
    static MonoMethod *method;

    if (method == NULL) {
        method = LoadEvent("OnTick", 0);
    }

    CallEvent(method, NULL);
}

MonoMethod *SampSharp::LoadEvent(const char *name, int param_count) {
	MonoMethod *method = mono_class_get_method_from_name(gamemode.klass, name, param_count);

	if (!method) {
		method = mono_class_get_method_from_name(basemode.klass, name, param_count);
	}

	return method;
}

int SampSharp::GetParamLengthIndex(MonoMethod *method, int idx) {
    static MonoClass *param;
    static MonoMethod *param_get;

    if(!param) {
        param = mono_class_from_name(basemode.image, "SampSharp.GameMode", "ParameterLengthAttribute");
    }
    if(!param_get) {
	    param_get = mono_property_get_get_method(mono_class_get_property_from_name(param, "Index"));
    }

    assert(param);
    assert(param_get);

	MonoCustomAttrInfo *attr = mono_custom_attrs_from_param(method, idx + 1);
	if (!attr) {
		ofstream logfile;
        
		logfile.open("SampSharp_errors.log", ios::app);
		cout << "[SampSharp] ERROR: No attribute info for " << mono_method_get_name(method)
            << "@" << idx << endl;
		logfile << TimeUtil::GetTimeStamp() << "ERROR: No attribute info for "
            << mono_method_get_name(method) << "@" << idx << endl;
		logfile.close();
		return -1;
	}

	MonoObject *attrObj = mono_custom_attrs_get_attr(attr, param);
	if (!attrObj) {
		ofstream logfile;
		logfile.open("SampSharp_errors.log", ios::app);
		cout << "[SampSharp] ERROR: Array parameter has no specified size: "
            << mono_method_get_name(method) << "@" << idx << endl;
		logfile << TimeUtil::GetTimeStamp() << "ERROR: Array parameter has no specified size: "
            << mono_method_get_name(method) << "@" << idx << endl;
		logfile.close();
		return -1;
	}

	return *(int*)mono_object_unbox(mono_runtime_invoke(param_get, attrObj, NULL, NULL));
}
bool SampSharp::ProcessPublicCall(AMX *amx, const char *name, cell *params, cell *retval) {
	const int param_count = params[0] / sizeof(cell);
	if (strlen(name) == 0 || param_count > 16) {
		return true;
	}

	mono_thread_attach(root);

	//detect unknown methods
	if (events.find(name) == events.end()) {
		//find method
		MonoMethod *method = mono_class_get_method_from_name(gamemode.klass, name, param_count);
		
		MonoImage *image = gamemode.image;
		if (!method) {
			method = mono_class_get_method_from_name(basemode.klass, name, param_count);
			image = basemode.image;
		}

		if (!method){
			events[name] = NULL;
			return true;
		}

		//iterate params
		void *iter = NULL;
		int iter_idx = 0;
		ParamMap params;

		MonoType* type = NULL;
		MonoMethodSignature *sig = mono_method_get_signature(method, image, 
            mono_method_get_token(method));

		while (type = mono_signature_get_params(sig, &iter)) {
			char *type_name = mono_type_get_name(type);
            param_t *par = new param_t;

			if (!strcmp(type_name, "System.Int32")) {
				par->type = PARAM_INT;
			}
			else if (!strcmp(type_name, "System.Single")) {
				par->type = PARAM_FLOAT;
			}
			else if (!strcmp(type_name, "System.String")) {
				par->type = PARAM_STRING;
			}
			else if (!strcmp(type_name, "System.Boolean")) {
				par->type = PARAM_BOOL;
			}
			else if (!strcmp(type_name, "System.Int32[]")) {
				par->type = PARAM_INT_ARRAY;
				
				int index = GetParamLengthIndex(method, iter_idx);
				if (index == -1) {
					events[name] = NULL;
					return true;
				}
				par->length_idx = index;
			}
			else if (!strcmp(type_name, "System.Single[]")) {
				par->type = PARAM_FLOAT_ARRAY;

				int index = GetParamLengthIndex(method, iter_idx);
				if (index == -1) {
	                ofstream logfile;
				    logfile.open("SampSharp_errors.log", ios::app);
				    cout << "[SampSharp] ERROR: No parameter length provided: "
                        << type_name << " in " << name << endl;
				    logfile << TimeUtil::GetTimeStamp() << "ERROR: No parameter length provided: "
                        << type_name << " in " << name << endl;
				    logfile.close();
				    events[name] = NULL;
				    return true;
				}
				par->length_idx = index;
			}
			else if (!strcmp(type_name, "System.Boolean[]")) {
				par->type = PARAM_BOOL_ARRAY;

				int index = GetParamLengthIndex(method, iter_idx);
				if (index == -1) {
				    ofstream logfile;
				    logfile.open("SampSharp_errors.log", ios::app);
				    cout << "[SampSharp] ERROR: No parameter length provided: "
                        << type_name << " in " << name << endl;
				    logfile << TimeUtil::GetTimeStamp() << "ERROR: No parameter length provided: "
                        << type_name << " in " << name << endl;
				    logfile.close();
				    events[name] = NULL;
				    return true;
				}
				par->length_idx = index;
			}
			else {
				ofstream logfile;
				logfile.open("SampSharp_errors.log", ios::app);
				cout << "[SampSharp] ERROR: Incompatible parameter type: "
                    << type_name << " in " << name << endl;
				logfile << TimeUtil::GetTimeStamp() << "ERROR: Incompatible parameter type: "
                    << type_name << " in " << name << endl;
				logfile.close();
				events[name] = NULL;
				return true;
			}
            
			params[iter_idx++] = par;
		}
		
		event_t *event_add = new event_t;
		event_add->method = method;
		event_add->params = params;
		events[name] = event_add;
	}

	event_t *event_p = events[name];
	//call known events
	if (event_p) {
		if (!param_count) {
			int retint = SampSharp::CallEvent(event_p->method, NULL);
			if (retint != -1) {
				*retval = retint;
			}
			return false;
		}
		else {
			void *args[16];
			int len = NULL;
			cell *addr = NULL;
			MonoArray *arr;

			for (int i = 0; i < param_count; i++) {
                switch (event_p->params[i]->type) {
                case PARAM_INT:
                case PARAM_FLOAT:
                case PARAM_BOOL: {
                    args[i] = &params[i + 1];
                    break;
                }
                case PARAM_STRING: {
                    amx_GetAddr(amx, params[i + 1], &addr);
                    amx_StrLen(addr, &len);

                    if (len) {
                        len++;

                        char* text = new char[len];

                        amx_GetString(text, addr, 0, len);
                        args[i] = mono_string_new(mono_domain_get(), text);
                    }
                    else {
                        args[i] = mono_string_new(mono_domain_get(), "");
                    }
                    break;
                }
                case PARAM_INT_ARRAY: {
                    len = params[event_p->params[i]->length_idx];
                    arr = mono_array_new(mono_domain_get(), mono_get_int32_class(), len);

                    if (len > 0) {
                        cell* addr = NULL;
                        amx_GetAddr(amx, params[i + 1], &addr);

                        for (int i = 0; i < len; i++) {
                            mono_array_set(arr, int, i, *(addr + i));
                        }
                    }
                    args[i] = arr;
                    break;
                }
                case PARAM_FLOAT_ARRAY: {
                    len = params[event_p->params[i]->length_idx];
                    arr = mono_array_new(mono_domain_get(), mono_get_int32_class(), len);

                    if (len > 0) {
                        cell* addr = NULL;
                        amx_GetAddr(amx, params[i + 1], &addr);

                        for (int i = 0; i < len; i++) {
                            mono_array_set(arr, float, i, amx_ctof(*(addr + i)));
                        }
                    }
                    args[i] = arr;
                    break;
                }
                case PARAM_BOOL_ARRAY: {
                    len = params[event_p->params[i]->length_idx];
                    arr = mono_array_new(mono_domain_get(), mono_get_int32_class(), len);

                    if (len > 0) {
                        cell* addr = NULL;
                        amx_GetAddr(amx, params[i + 1], &addr);

                        for (int i = 0; i < len; i++) {
                            mono_array_set(arr, bool, i, !!*(addr + i));
                        }
                    }
                    args[i] = arr;
                    break;
                }
                default:
                    assert(0 && "Invalid type specifier");
                    break;
                }
			}

			int retint = SampSharp::CallEvent(event_p->method, args);

			if (retint != -1) {
				*retval = retint;
			}
			return false;
		}
		return false;
	}

	return true;
}

int SampSharp::CallEvent(MonoMethod* method, void **params) {
    assert(method);

    MonoObject *exception;
	MonoObject *response = mono_runtime_invoke(method, mono_gchandle_get_target(gameModeHandle),
        params, &exception);

	if (exception) {
		char *stacktrace = mono_string_to_utf8(mono_object_to_string(exception, NULL));

		ofstream logfile;
		logfile.open("SampSharp_errors.log", ios::app | ios::binary);
		cout << "[SampSharp] Exception thrown:" << endl << stacktrace << endl;
		logfile << TimeUtil::GetTimeStamp() << " Exception thrown:" << "\r\n" << stacktrace << "\r\n";
		logfile.close();

		return -1;
	}

	if (!response) {
		return -1;
	}

	return *(bool *)mono_object_unbox(response) == true ? 1 : 0;
}

void SampSharp::Unload() {
	mono_jit_cleanup(mono_domain_get());
}
