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
using System.Linq;

namespace org.herbal3d.cs.CommonUtil {
    public class Util {
        public static T Clamp<T>(T x, T min, T max)
            where T : IComparable<T>
        {
            return x.CompareTo(max) > 0 ? max :
                x.CompareTo(min) < 0 ? min :
                x;
        }

        // Returns a string of the passed length of a random.
        // Note that this is not cryptographically random.
        static Random _randomStringRandom = new Random();
        public static string RandomString(int pLen) {
            int len = Clamp<int>(pLen, 1, 128);
            string digits = "0123456789";
            return String.Join("", Enumerable.Range(0, len).Select( ii => {
                return digits[_randomStringRandom.Next(0, 10)];
            }) );
        }

    }
}
