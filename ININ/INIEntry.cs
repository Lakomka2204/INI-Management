using System;
using System.Globalization;
namespace ININ
{
    /// <summary>
    /// Represents class of single INI entry
    /// </summary>
    public class INIEntry
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="section">Section</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public INIEntry(string section, string key, object value)
        {
            Section = section.Trim();
            Key = key.Trim();
            Value = value;
        }
        /// <summary>
        /// Converts <see cref="Value"/> if it's <see cref="double"/> type
        /// </summary>
        /// <remarks>To check if <see cref="Value"/> is <see cref="double"/> use <see cref="IsNumber"/> property</remarks>
        /// <returns><see cref="Value"/> converted to <see cref="double"/></returns>
        public double ToNumber()
        {
            if (IsNumber())
                return Convert.ToDouble(Value);
            else return double.MinValue;
        }
        /// <summary>
        /// Section of INI entry
        /// </summary>
        public string Section { get; set; }
        /// <summary>
        /// Key of INI entry
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Checks if <see cref="Value"/> is <see cref="double"/>
        /// </summary>
        /// <returns>true if <see cref="Value"/> is <see cref="double"/>, false if not</returns>
        public bool IsNumber()
            => double.TryParse(Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        /// <summary>
        /// Value of INI entry
        /// </summary>
        public object Value { get; set; }
        /// <summary>
        /// Returns string representing INI entry
        /// </summary>
        /// <returns>[<see cref="Section"/>] <see cref="Key"/>="<see cref="Value"/>"</returns>
        public override string ToString()
            => $"[{Section}] {Key}={Value}";
    }
}
