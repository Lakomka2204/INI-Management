using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
namespace ININ
{
    /// <summary>
    /// Represents class for ini files management
    /// </summary>
    public class INI : IDisposable
    {
        /// <summary>
        /// Opens ini file for management, if doesn't exists, creates one
        /// </summary>
        /// <param name="filename">File path</param>
        /// <exception cref="IOException">File exception</exception>
        public INI(string filename)
        {
            if (!File.Exists(filename))
                File.CreateText(filename).Close();
            entries = ParseINI(filename);
            this.filename = filename;
            disposed = modified = false;
            INIMode = INIMode.UpdateOnAction;
        }
        /// <summary>
        /// Opens ini file for management, if doesn't exists, creates one
        /// </summary>
        /// <remarks>
        /// <para>Note:</para>
        /// <see cref="INIMode.UpdateOnAction"/>
        /// will update ini file on every action e.g. <see cref="SetValue(INIEntry)"/>, <see cref="DeleteKey(string, string)"/>
        /// <para>
        /// <see cref="INIMode.UpdateOnDispose"/>
        /// will update ini file after object disposal
        /// </para>
        /// </remarks>
        /// <param name="filename">File path</param>
        /// <param name="mode">INI edit mode</param>
        public INI(string filename, INIMode mode) : this(filename)
        {
            INIMode = mode;
        }
        /// <summary>
        /// Gets all sections from accosiated ini file
        /// </summary>
        /// <returns><see cref="Array"/> of sections</returns>
        public string[] GetSections()
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            return entries.GroupBy(x => x.Section).Select(x => x.Key).ToArray();
        }
        /// <summary>
        /// Gets all keys from <paramref name="section"/> from accosiated ini file
        /// </summary>
        /// <param name="section">Section</param>
        /// <returns><see cref="Array"/> of keys</returns>
        public string[] GetKeys(string section)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            return entries.Where(x => x.Section == section).Select(x => x.Key).ToArray();
        }
        /// <summary>
        /// Gets string from ini file
        /// </summary>
        /// <remarks>Returns null if <paramref name="section"/>/<paramref name="key"/> is not found</remarks>
        /// <param name="section">Section</param>
        /// <param name="key">Key</param>
        /// <returns><see cref="string"/> that matches required arguments</returns>
        public string GetStringValue(string section, string key)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            if (INIMode == INIMode.UpdateOnAction)
            entries = ParseINI(filename);
            var i = GetEntry(section, key);
            if (i == null) return null;
            else return i.Value.ToString();
        }
        /// <summary>
        /// Gets <see cref="double"/> number from ini file
        /// </summary>
        /// <remarks>Returns <see cref="double.MaxValue"/> if <paramref name="section"/>/<paramref name="key"/> is not found or <see cref="double.MinValue"/> if matched value is not a number</remarks>
        /// <param name="section">Section</param>
        /// <param name="key">Key</param>
        /// <returns><see cref="double"/> that matches required arguments</returns>
        public double GetNumberValue(string section, string key)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            if (INIMode == INIMode.UpdateOnAction)
                entries = ParseINI(filename);
            var i = GetEntry(section, key);
            if (i == null) return double.MaxValue;
            return i.ToNumber();
        }
        /// <summary>
        /// Sets <paramref name="value"/> to <paramref name="key"/> of <paramref name="section"/> or inserts new <paramref name="value"/> if <paramref name="section"/>
        /// with <paramref name="key"/> already exists into accosiated ini file
        /// </summary>
        /// <param name="section">Section</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Current <see cref="INI"/> for next usage e.g. <code>
        /// <para><see cref="INI"/> obj = <see cref="new"/> <see cref="INI"/>("settings.ini");</para>
        /// <para>obj.<see cref="SetValue"/></para>
        /// <para>.<see cref="SetValue"/></para>
        /// <para>...</para>
        /// </code></returns>
        public INI SetValue(string section, string key, object value)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            modified = true;
            var i = GetEntry(section, key);
            if (i == null)
            {
                i = new INIEntry(section, key, value.ToString());
                entries.Add(i);
            }
            else
            {
                int p = entries.IndexOf(i);
                entries[p].Value = value;
            }
            if (INIMode == INIMode.UpdateOnAction)
                Apply();
            return this;
        }
        /// <summary>
        /// Sets <see cref="INIEntry"/> or inserts new value if <see cref="INIEntry.Section"/> with <see cref="INIEntry.Key"/>
        /// already exists into accosiated ini file
        /// </summary>
        /// <param name="entry">Single INI entry</param>
        /// <returns>Current<see cref="INI"/> for next usage e.g. <code>
        /// <para><see cref="INI"/> obj = <see cref="new"/> <see cref="INI"/>("settings.ini");</para>
        /// <para>obj.<see cref="SetValue(INIEntry)"/></para>
        /// <para>.<see cref="SetValue(INIEntry)"/></para>
        /// <para>...</para>
        /// </code></returns>
        public INI SetValue(INIEntry entry)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            modified = true;
            var i = GetEntry(entry.Section, entry.Key);
            if (i == null)
                entries.Add(entry);
            else
            {
                int p = entries.IndexOf(i);
                entries[p] = entry;
            }
            if (INIMode == INIMode.UpdateOnAction)
                Apply();
            return this;
        }
        /// <summary>
        /// Sets multiple <see cref="INIEntry"/> or inserts new
        /// </summary>
        /// <param name="entries">Multiple INI entries</param>
        /// <returns>Current <see cref="INI"/> for next usage e.g. <code>
        /// <para><see cref="INI"/> obj = <see cref="new"/> <see cref="INI"/>("settings.ini");</para>
        /// <para>obj.<see cref="SetValues(INIEntry[])"/></para>
        /// <para>.<see cref="SetValues(INIEntry[])"/></para>
        /// <para>...</para>
        /// </code></returns>
        public INI SetValue(params INIEntry[] entries)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            modified = true;
            foreach (var e in entries)
                SetValue(e);
            return this;
        }
        /// <summary>
        /// Deletes section and all of it's entries
        /// </summary>
        /// <param name="section">Section</param>
        /// <returns>Number of elemets removed</returns>
        public int DeleteSection(string section)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            modified = true;
            int r = entries.RemoveAll(x => x.Section == section);
            if (INIMode == INIMode.UpdateOnAction)
                Apply();
            return r;
        }
        /// <summary>
        /// Deletes <paramref name="key"/> and it's value
        /// </summary>
        /// <param name="section">Section</param>
        /// <param name="key">Key</param>
        /// <returns>true if key is successfully deleted, false if not found</returns>
        public bool DeleteKey(string section, string key)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            modified = true;
            var i = GetEntry(section, key);
            if (i == null) return false;
            bool a = entries.Remove(i);
            if (INIMode == INIMode.UpdateOnAction)
                Apply();
            return a;
        }
        /// <summary>
        /// Checks if <paramref name="section"/> exists into accosiated ini file
        /// </summary>
        /// <param name="section">Section</param>
        /// <returns>true if <paramref name="section"/> exists, false if not</returns>
        public bool SectionExists(string section)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            return entries.Any(a => a.Section == section);
        }
        /// <summary>
        /// Checks if <paramref name="key"/> of <paramref name="section"/>
        /// exists into accosiated ini file
        /// </summary>
        /// <param name="section">Section</param>
        /// <param name="key">Key</param>
        /// <returns>true if <paramref name="key"/> of <paramref name="section"/> exists, false if not</returns>
        public bool KeyExists(string section, string key)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            return entries.Any(a => a.Section == section && a.Key == key);
        }
        /// <summary>
        /// Checks if value from <paramref name="key"/> of <paramref name="section"/> is number type
        /// </summary>
        /// <param name="section">Section</param>
        /// <param name="key">Key</param>
        /// <returns>true if value type is <see cref="double"/>, false if value type is <see cref="string"/></returns>
        public bool IsNumber(string section, string key)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            var i = GetEntry(section, key);
            return i?.IsNumber() ?? false;
        }
        public override bool Equals(object obj)
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            return obj is INI && (obj as INI).filename == filename;
        }
        public override string ToString()
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            return Directory.GetParent(filename).FullName + Path.DirectorySeparatorChar + filename;
        }
        public override int GetHashCode()
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            int hashCode = -1855475341;
            return hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(filename);
        }
        public void Dispose()
        {
            if (disposed) throw new ObjectDisposedException(nameof(INI));
            if (modified)
                Apply();
            entries.Clear();
            filename = null;
            disposed = true;
        }
        List<INIEntry> ParseINI(string filename)
        {
            List<INIEntry> entries = new List<INIEntry>();
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);
            using (var sr = new StreamReader(filename))
            {
                Regex rSection = new Regex(@"\[(.*)\]"),
                    rKeyValNoBrackets = new Regex(@"(.*)=(.*)"),
                    rKeyVal = new Regex(@"(.*)=(""(.*)"")");
                string cursection = "";
                while (!sr.EndOfStream)
                {
                    string cstr = sr.ReadLine();
                    Match msec = rSection.Match(cstr),
                        mkeyvalnobra = rKeyValNoBrackets.Match(cstr),
                        mkeyval = rKeyVal.Match(cstr);
                    if (msec.Success)
                    {
                        cursection = msec.Groups[1].Value;
                    }
                    else if (mkeyval.Success || mkeyvalnobra.Success)
                    {
                        foreach (Match i in mkeyval.Captures.Count > 0 ? mkeyval.Captures : mkeyvalnobra.Captures)
                        {
                            string key = i.Groups[1].Value,
                                val = i.Groups[mkeyval.Success ? 3 : 2].Value;
                            var xs = entries.FirstOrDefault(x => x.Key == key && x.Section == cursection);
                            if (xs != null)
                            {
                                int pos = entries.IndexOf(xs);
                                entries.RemoveAt(pos);
                                entries.Insert(pos, new INIEntry(cursection, key, val));
                            }
                            else entries.Add(new INIEntry(cursection, key, val));
                        }
                    }
                }
            }
            return entries;
        }
        INIEntry GetEntry(string s, string k)
            => entries.FirstOrDefault(x => x.Section == s && x.Key == k);
        void Apply()
        {
            string lastsection = "";
            using (StreamWriter sw = new StreamWriter(filename))
            {
                foreach (INIEntry e in entries)
                {
                    sw.WriteLine(lastsection == e.Section
                        ? $"{e.Key}={(e.IsNumber() ? e.Value : $"\"{e.Value}\"")}" : $"[{e.Section}]\n{e.Key}={(e.IsNumber() ? e.Value : $"\"{e.Value}\"")}");
                    lastsection = e.Section;
                }
            }
            entries = ParseINI(filename);
        }
        List<INIEntry> entries;
        string filename;
        bool disposed,
            modified;
        public INIMode INIMode { get; set; }
    }
    public enum INIMode
    {
        UpdateOnAction,
        UpdateOnDispose
    }
}
