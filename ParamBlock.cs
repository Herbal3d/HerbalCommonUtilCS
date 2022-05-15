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
using System.Text;

namespace org.herbal3d.cs.CommonUtil {
    // ParamBlock is a collection of key/value pairs.
    public class ParamBlock : IParameters {

        // Params are kept in two dictionaries with a main name and a lower case name
        public Dictionary<string, object> Params = new Dictionary<string, object>();
        public Dictionary<string, object> LowerParams = new Dictionary<string, object>();

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
                    SetParam(paramName, pConfigParams.GetObjectValue(paramName));
                }
                if (pPassedParams != null && pPassedParams.HasParam(paramName)) {
                    // parameter value has been passed so it override config file
                    SetParam(paramName, pPassedParams.GetValue(paramName));
                }
                // If the above doesn't define a value, enter the default value
                if (!Params.ContainsKey(paramName)) {
                    SetParam(paramName, pRequiredParams.Params[paramNameX]);
                }
            }
        }

        public bool HasParam(string pParamName) {
            string key = pParamName.ToLower();
            return LowerParams.ContainsKey(key);
        }
        public void SetParam(string pParamName, object pVal) {
            string key = pParamName.ToLower();
            lock (Params) {
                if (LowerParams.ContainsKey(key)) {
                    LowerParams[key] = pVal;
                    foreach (var kvp in Params) {
                        if (kvp.Key.ToLower() == key) {
                            Params[kvp.Key] = pVal;
                            break;
                        }
                    }
                }
                else {
                    Params.Add(pParamName, pVal);
                    LowerParams.Add(key, pVal);
                }
            }
        }
        public object GetValue(string pParamName) {
            object val = null;
            string key = pParamName.ToLower();
            if (LowerParams.ContainsKey(key)) {
                val = LowerParams[key];
            }
            return val;
        }

        // Get the parameter value of the type desired
        public T P<T>(string pParam) {
            T ret = default;
            if (LowerParams.TryGetValue(pParam.ToLower(), out object val)) {
                ret = ConvertTo<T>(val);
            }
            return ret;
        }

        public object GetObjectValue(string pParamName) {
            LowerParams.TryGetValue(pParamName.ToLower(), out object ret);
            return ret;
            
        }

        public ParamBlock Add(string pName, Object pValue) {
            lock (Params) {
                Params.Add(pName, pValue);
                LowerParams.Add(pName.ToLower(), pValue);
            }
            return this;
        }

        // Add values from another ParamBlock to this ParamBlock
        public ParamBlock Add(ParamBlock pParams) {
            foreach (var kvp in pParams.Params) {
                this.Add(kvp.Key, kvp.Value);
            }
            return this;
        }

        public void Remove(string pName) {
            string pNameLower = pName.ToLower();
            lock (Params) {
                if (LowerParams.ContainsKey(pNameLower)) {
                    LowerParams.Remove(pNameLower);
                    foreach (var kvp in Params) {
                        if (kvp.Key.ToLower() == pNameLower) {
                            Params.Remove(kvp.Key);
                            break;
                        }
                    }
                }
            }
        }

        // Copy the parameters from this ParamBlock into a dictionary
        public void CopyTo(Dictionary<string, object> pDst) {
            lock (Params) {
                foreach (var kvp in Params) {
                    pDst.Add(kvp.Key, kvp.Value);
                }
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

        public static object ConvertToObj(Type pT, object pVal) {
            object ret = null;
            if (pVal.GetType() == pT) {
                ret = pVal;
            }
            else {
                try {
                    //Handling Nullable types i.e, int?, double?, bool? .. etc
                    if (Nullable.GetUnderlyingType(pT) != null) {
                        ret = TypeDescriptor.GetConverter(pT).ConvertFrom(pVal);
                    }
                    else {
                        if (pVal.GetType().GetMethod("ConvertTo") != null) {
                            ret = pVal.GetType().GetMethod("ConvertTo").Invoke(pVal, new object[] { pT });
                        }
                        else {
                            ret = Convert.ChangeType(pVal, pT);
                        }
                    }
                }
                catch (Exception) {
                    ret = default;
                }
            }
            return ret;
        }

        public string DumpProps() {
            StringBuilder buff = new StringBuilder();
            foreach (var kvp in Params) {
                if (buff.Length > 0) {
                    buff.Append(",");
                }
                buff.Append(kvp.Key);
                buff.Append("=");
                buff.Append(ConvertTo<string>(kvp.Key));
            }
            return buff.ToString();
        }
    }
}
