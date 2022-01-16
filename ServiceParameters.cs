using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace org.herbal3d.cs.CommonUtil {

    // Base parameter definition that gets and sets parameter values via a string
    public abstract class ParameterDefnBase
    {
        public string name;         // string name of the parameter
        public string desc;         // a short description of what the parameter means
        public abstract Type GetValueType();
        public string[] symbols;    // extra command line versions of parameter (short form)
        public BLogger logger;      // used only for debugging and error output
        public string _logHeader = "[ParameterDefnBase]";

        public ParameterDefnBase(string pName, string pDesc, string[] pSymbols)
        {
            name = pName;
            desc = pDesc;
            symbols = pSymbols;
        }
        // Set the parameter value to the default
        public abstract void AssignDefault();
        // Get the value as a string
        public abstract string GetValue();
        // Get the value as just an object
        public abstract object GetObjectValue();
        // Set the value to this string value
        public abstract void SetValue(string valAsString);
    }

    // Specific parameter definition for a parameter of a specific type.
    public sealed class ParameterDefn<T> : ParameterDefnBase
    {
        private readonly T defaultValue;
        private T value;

        public ParameterDefn(string pName, string pDesc, T pDefault, params string[] pSymbols)
            : base(pName, pDesc, pSymbols) {
            defaultValue = pDefault;
            _logHeader = "[ParameterDefn:" + pName + "]";   // makes error messages findable
        }
        public T Value() {
            return value;
        }

        public override void AssignDefault() {
            value = defaultValue;
        }
        public override string GetValue()
        {
            string ret = String.Empty;
            if (value != null) {
                ret = value.ToString();
            }
            return ret;
        }
        public override Type GetValueType() {
            return typeof(T);
        }
        public override object GetObjectValue() {
            return value;
        }
        public override void SetValue(String valAsString) {
            // Find the 'Parse' method on that type
            System.Reflection.MethodInfo parser;
            try {
                parser = GetValueType().GetMethod("Parse", new Type[] { typeof(String) });
            }
            catch {
                parser = null;
            }
            if (parser != null) {
                // Parse the input string
                try {
                    T setValue = (T)parser.Invoke(GetValueType(), new Object[] { valAsString });
                    // System.Console.WriteLine("SetValue: setting value on {0} to {1}", this.name, setValue);
                    // Store the parsed value
                    value = setValue;
                    // logger.log.DebugFormat("{0} SetValue. {1} = {2}", _logHeader, name, setValue);
                }
                catch (Exception e) {
                    logger.Error("{0} Failed parsing parameter value '{1}': '{2}'", _logHeader, valAsString, e);
                }
            }
            else {
                // If there is not a parser, try doing a conversion
                try {
                    T setValue = (T)Convert.ChangeType(valAsString, GetValueType());
                    value = setValue;
                    logger.Debug("{0} SetValue. Converter. {1} = {2}", _logHeader, name, setValue);
                }
                catch (Exception e) {
                    logger.Error("{0} Conversion failed for {1}: {2}", _logHeader, this.name, e);
                }
            }
        }
    }

    public class ServiceParameters: IParameters {

        protected readonly BLogger _logger;          // used for debugging and error reporting
        protected string _logHeader = "[ServiceParameters]";

        // The predefined parameters for this service
        protected ParameterDefnBase[] ParameterDefinitions;

        public ServiceParameters(BLogger pLogger) {
            _logger = pLogger;
            SetParameterDefaultValues(_logger);
        }

        // Return a value for the parameter.
        // This is used by most callers to get parameter values.
        // Note that it outputs a console message if not found. Not found means that the caller
        //     used the wrong string name.
        public T P<T>(string paramName) {
            T ret = default;
            if (TryGetParameter(paramName, out ParameterDefnBase pbase)) {
                if (pbase is ParameterDefn<T> pdef) {
                    ret = pdef.Value();
                }
                else {
                    _logger.Error("{0} Fetched parameter of wrong type. Param={1}", _logHeader, paramName);
                }
            }
            else {
                _logger.Error("{0} Fetched unknown parameter. Param={1}", _logHeader, paramName);
            }
            return ret;
        }
        public bool HasParam(string pParamName) {
            return TryGetParameter(pParamName, out _);
        }

        public object GetObjectValue(string pParamName) {
            object ret = null;
            if (TryGetParameter(pParamName, out ParameterDefnBase pbase)) {
                ret = pbase.GetObjectValue();
            }
            return ret;
        }

        // This thing doesn't do a remove
        public void Remove(string pParamName) {
            throw new NotImplementedException();
        }

        // Search through the parameter definitions and return the matching
        //    ParameterDefn structure.
        // Case does not matter as names are compared after converting to lower case.
        // Returns 'false' if the parameter is not found.
        public bool TryGetParameter(string paramName, out ParameterDefnBase defn)
        {
            bool ret = false;
            ParameterDefnBase foundDefn = null;
            string pName = paramName.ToLower();

            foreach (ParameterDefnBase parm in ParameterDefinitions)
            {
                if (pName == parm.name.ToLower())
                {
                    foundDefn = parm;
                    ret = true;
                    break;
                }
            }
            defn = foundDefn;
            return ret;
        }

        // Pass through the settable parameters and set the default values
        public void SetParameterDefaultValues(BLogger pLogger)
        {
            foreach (ParameterDefnBase parm in ParameterDefinitions)
            {
                parm.logger = pLogger;
                parm.AssignDefault();
            }
        }

    }
}
