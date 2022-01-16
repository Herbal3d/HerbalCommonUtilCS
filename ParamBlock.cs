/*
 * Copyright (c) 2021 Robert Adams
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace org.herbal3d.cs.CommonUtil {
    // ParamBlock is a collection of key/value pairs.
    public class ParamBlock : IParameters {

        public Dictionary<string, object> Params = new Dictionary<string, object>();

        public ParamBlock() {
        }

        public ParamBlock(Dictionary<string, object> pPreload) {
            foreach (var kvp in pPreload) {
                Add(kvp.Key, kvp.Value);
            }
        }

        // Configuration comes from the configuration file (Config), parameters at
        //    may be set in the context, and parameters that may be required.
        //    This takes those three inputs and creates one parameter block with
        //    the proper merge of those three sources.
        // Adding this to a routine documents the parameters that are expected
        //    by the routine.
        // Passed context parameters take highest priority, then config file, then
        //    default/required values.
        // NOTE: everything is forced to all lowercase thus the resulting value
        //    lookup MUST be looking for a lowercase only value.
        public ParamBlock(IParameters pConfigParams,
                          ParamBlock pPassedParams,
                          ParamBlock pRequiredParams) {

            // For each of the required parameters, find a value
            foreach (var kvp in pRequiredParams.Params) {
                string paramNameX = kvp.Key;    // the original for those who use camel case
                string paramName = kvp.Key.ToLower();   // force name to uniform lower case

                if (pConfigParams != null && pConfigParams.HasParam(paramName)) {
                    // Required parameter value is in application parameters. Start with that value.
                    Params.Add(paramName, pConfigParams.GetObjectValue(paramName));
                }
                if (pPassedParams != null
                            && (pPassedParams.Params.ContainsKey(paramName)
                                || pPassedParams.Params.ContainsKey(paramNameX)) ) {
                    // parameter value has been passed so it override config file
                    if (!pPassedParams.Params.TryGetValue(paramNameX, out object vall)) {
                        pPassedParams.Params.TryGetValue(paramName, out vall);
                    }
                    if (Params.ContainsKey(paramName)) {
                        Params[paramName] = vall;
                    }
                    else {
                        Params.Add(paramName, vall);
                    }
                }
                // If the above doesn't define a value, enter the default value
                if (pRequiredParams != null && !Params.ContainsKey(paramName)) {
                    Params.Add(paramName, pRequiredParams.Params[paramNameX]);
                }
            }
        }

        public bool HasParam(string pParamName) {
            return Params.ContainsKey(pParamName.ToLower());
        }

        // Get the parameter value of the type desired
        public T P<T>(string pParam) {
            T ret = default;
            if (Params.TryGetValue(pParam.ToLower(), out object val)) {
                ret = ConvertTo<T>(val);
            }
            return ret;
        }

        public object GetObjectValue(string pParamName) {
            Params.TryGetValue(pParamName.ToLower(), out object ret);
            return ret;
            
        }

        public ParamBlock Add(string pName, Object pValue) {
            Params.Add(pName.ToLower(), pValue);
            return this;
        }

        public void Remove(string pName) {
            string pNameLower = pName.ToLower();
            if (Params.ContainsKey(pNameLower)) {
                Params.Remove(pNameLower);
            }
        }
        

        // Cool routine to convert an object to the request type
        // https://stackoverflow.com/questions/3502493/is-there-any-generic-parse-function-that-will-convert-a-string-to-any-type-usi
        public static T ConvertTo<T>(object val) {
            T ret;
            if (val is T variable) {
                // If the type is not changing, just return the 'object' converted
                ret = variable;
            }
            else {
                try {
                    //Handling Nullable types i.e, int?, double?, bool? .. etc
                    if (Nullable.GetUnderlyingType(typeof(T)) != null) {
                        ret = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(val);
                    }
                    else {
                        if (val.GetType().GetMethod("ConvertTo") != null) {
                            ret = (T)val.GetType().GetMethod("ConvertTo").Invoke(val, new object[] { typeof(T) });
                        }
                        else {
                            ret = (T)Convert.ChangeType(val, typeof(T));
                        }
                    }
                }
                catch (Exception) {
                    ret = default;
                }
            }
            return ret;
        }
    }
}
