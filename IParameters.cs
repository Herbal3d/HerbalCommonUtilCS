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

namespace org.herbal3d.cs.CommonUtil {
    public interface IParameters {
        // Basic parameter fetching interface which returns the parameter
        //   value of the requested parameter name.
        T P<T>(string pParamName);
        object GetObjectValue(string pParamName);
        bool HasParam(string pParamName);
        void Remove(string pParamName);
    }

}
