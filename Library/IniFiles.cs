using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using vnebo.mobi.bot.Libs;
using System.Linq;
using System.Text;

namespace vnebo.mobi.bot
{
    internal class IniFiles
    {
        

        private struct SectionPair : IComparable<SectionPair>
        {
            public string Section;
            public string Key;
            public int CompareTo(SectionPair other)
            {
                return string.Compare(Key, other.Key);
            }
        }
        public class CompareSection : IComparer
        {
            int IComparer.Compare(Object s1, Object s2)
            {
                string num1 = s1.ToString();
                num1 = new Regex("USER_([0-9]+)").Match(num1).Groups[1].Value;
                string num2 = s2.ToString();
                num2 = new Regex("USER_([0-9]+)").Match(num2).Groups[1].Value;
                if (int.TryParse(num1, out int res1)&& int.TryParse(num2, out int res2))              
                    return res1.CompareTo(res2);
                return 1;
            }
        }

        #region ПЕРЕМЕННЫЕ
        public readonly string Path = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "settings.ini").FullName.ToString();
        public readonly Hashtable keyPairs = new Hashtable();
        #endregion

        public IniFiles()
        {
            //if (!File.Exists(Path)) { File.Create(Path); }
            //считали настройки
            IniParser();

        }
        public bool IniParser()
        {
   
            TextReader iniFile = null;
            string strLine;
            string currentRoot = null;
            string[] keyPair;
            keyPairs.Clear();
            
                try
                {
                    Stream myStream;
                    using (myStream = File.Open(Path, FileMode.OpenOrCreate, FileAccess.Read))
                { 
                    iniFile = new StreamReader(myStream);

                    strLine = iniFile.ReadLine();

                    while (strLine != null)
                    {
                        strLine = strLine.Trim();

                        if (strLine != "")
                        {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            }
                            else
                            {
                                keyPair = strLine.Split(new char[] { '=' }, 2);

                                SectionPair sectionPair;
                                string value = null;

                                if (currentRoot == null)
                                    currentRoot = "ROOT";

                                sectionPair.Section = currentRoot;
                                sectionPair.Key = keyPair[0];

                                if (keyPair.Length > 1)
                                    value = keyPair[1];
                                if (!keyPairs.ContainsKey(sectionPair))
                                {
                                    keyPairs.Add(sectionPair, value);
                                }
                            }
                        }

                        strLine = iniFile.ReadLine();
                    }
                }
                
                return true;

                }
                catch (Exception ex)
                {
                string read_log_file = System.IO.Path.GetTempPath() + $"tmp_log.log";
                string read_log = "";
                if (File.Exists(read_log_file))
                {
                    read_log = File.ReadAllText(read_log_file);
                }
                read_log += "ex iniParser: " + ex + "\r\n";
                TextWriter tw = new StreamWriter(read_log_file);
                tw.Write(read_log);
                tw.Close();
                IniParser();
                return true;
                }
                finally
                {
                iniFile?.Close();
                    
                }

        }

        /// <summary>
        /// Returns the value for the given section, key pair.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <param name="settingName">Key name.</param>
        public string Read(string sectionName, string settingName)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName.ToUpper();
            sectionPair.Key = settingName.ToUpper();
            return (string)keyPairs[sectionPair];
        }

        /// <summary>
        /// Enumerates all lines for given section.
        /// </summary>
        /// <param name="sectionName">Section to enum.</param>
        public Dictionary<string, string> EnumSection(string sectionName)
        {
            Dictionary<string, string> tmpDic = new Dictionary<string, string>();
            //IniParser();
            string read_file;
            
            if (File.Exists(Path))
            {
                // Если файл настроек не пустой
                do
                {
                    System.Threading.Thread.Sleep(HelpMethod.getRandomNumber.Next(20, 1000));
                    read_file = File.ReadAllText(Path);

                } while (read_file.Length == 0);
                string Section = new Regex(sectionName + @"\]\r?\n(.+?)\r?\n[\[$]", RegexOptions.Singleline | RegexOptions.Multiline).Match(read_file).Groups[1].Value;

                foreach (Match item in new Regex(@"(.+?)=(.+?)\r?\n", RegexOptions.Singleline | RegexOptions.Multiline).Matches(Section))
                {
                    tmpDic.Add(item.Groups[1].Value, item.Groups[2].Value);
                    // Console.WriteLine(item.Groups[1].Value+" - "+ item.Groups[2].Value);
                }
                //int i = 0;
                /*if (!tmpDic.ContainsKey("LOGIN")) { tmpDic = EnumSection(sectionName);i++; }
                if (!tmpDic.ContainsKey("PASSWORD")) { tmpDic = EnumSection(sectionName);i++; }
                if (!tmpDic.ContainsKey("SERVER")) { tmpDic = EnumSection(sectionName); i++;}
                if (i > 10) { MessageBox.Show("Не могу найти настройку логина/пароля/сервера! "+sectionName); return tmpDic; }*/
            }
            IniParser();
            return tmpDic;

            /*SectionPair sectionPair;
            sectionPair.Section = sectionName.ToUpper();
            try
            {
                foreach (SectionPair pair in keyPairs.Keys)
                {
                    sectionPair.Key = pair.Key.ToUpper();
                    if (pair.Section == sectionName.ToUpper())
                    {
                        tmpDic.Add(pair.Key, (string)keyPairs[pair]);
                    }
                }
            }catch(Exception ex) { Console.WriteLine("ex enum = "+ex); }

            return tmpDic;*/
        }
        public Dictionary<string, string> GetSett(string sectionName)
        {
            Dictionary<string, string> tmpDic = new Dictionary<string, string>();
  //          ArrayList sections = new ArrayList();


                foreach (SectionPair sectionPair in keyPairs.Keys)
                {

                    if (sectionPair.Section == sectionName)
                    {
                    tmpDic.Add(sectionPair.Key, keyPairs[sectionPair].ToString());
                    }
                }

            return tmpDic;
        }
        /// <summary>
        /// Adds or replaces a setting to the table to be saved.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        /// <param name="settingValue">Value of key.</param>
        public void Write(string sectionName, string settingName, string settingValue)
        {
            sectionName = sectionName.ToUpper();
            sectionName = sectionName.ToUpper();
            SectionPair sectionPair;
            sectionPair.Section = sectionName;
            sectionPair.Key = settingName;

            if (keyPairs.ContainsKey(sectionPair))
                keyPairs.Remove(sectionPair);

            keyPairs.Add(sectionPair, settingValue);
            //SaveSettings();
            //IniParser();
            // Путь к файлу
            //string filePath = "путь_к_файлу.txt";



            // Read the file
            string[] lines = File.ReadAllLines(Path);

            // Store the groups and their lines in a Dictionary
            var groups = new Dictionary<string, List<string>>();
            string currentGroup = null;
            foreach (string line in lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentGroup = line.Substring(1, line.Length - 2);
                    groups[currentGroup] = new List<string>();

                }
                else if (currentGroup != null)
                {
                    groups[currentGroup].Add(line);
                }
            }
            if (!groups.ContainsKey(sectionName))
            {
                groups[sectionName] = new List<string>
                {
                    settingName + "=" + settingValue
                };
                Console.WriteLine("Добавили группу " + sectionName + " - " + settingName + "=" + settingValue);
            }


            // Update the parameter value in the specified group
            if (groups.TryGetValue(sectionName, out var groupLines))
            {
                Console.WriteLine("Нашли " + sectionName);
                for (int i = 0; i < groupLines.Count; i++)
                {
                    if (groupLines[i].StartsWith(settingName + "="))
                    {
                        groupLines[i] = settingName + "=" + settingValue;
                        break;
                    }
                }

                // If the parameter is not found, add it to the end of the group
                if (!groupLines.Any(line => line.StartsWith(settingName + "=")))
                {
                    groupLines.Add(settingName + "=" + settingValue);
                }
            }
            Console.WriteLine("Gr=" + sectionName + " - " + settingName + "=" + settingValue);
            // Rebuild the lines
            var newLines = new StringBuilder();
            foreach (var group in groups)
            {
                newLines.AppendLine("[" + group.Key + "]");
                foreach (var line in group.Value)
                {
                    newLines.AppendLine(line);
                }
            }
            try { 
            // Write the changes to the file
            File.WriteAllText(Path, newLines.ToString());
        } catch (Exception ex) { Console.WriteLine("ex write = " + ex); Write(sectionName, settingName, settingValue); }

        }
        public void DeleteSection(string sectionName)
        {
            string read_file = File.ReadAllText(Path);
            string sectionPattern = $@"(\[{sectionName}\]\r?\n.+?\r?\n)(\[|$|\r?\n)";
            Match sectionMatch = Regex.Match(read_file, sectionPattern, RegexOptions.Singleline);
            string section = sectionMatch.Groups[1].Value;

            try
            {
                if (string.IsNullOrEmpty(section))
                {
                    section = Regex.Match(read_file, $@"(\[{sectionName}\]\r?\n.+?\r?\n)$", RegexOptions.Singleline).Value;
                }
                File.WriteAllText(Path, read_file.Replace(section, ""));
            }
            catch (Exception ex)
            {
                string logFilePath = System.IO.Path.GetTempPath() + $"tmp_log.log";
                string logContent = File.Exists(logFilePath) ? File.ReadAllText(logFilePath) : "";
                logContent += $"ex del sect: {ex}\r\n";
                File.WriteAllText(logFilePath, logContent);
                DeleteSection(sectionName);
            }

            IniParser();
        }
        /*       public void DeleteSection(string sectionName)
               {
                   string read_file = File.ReadAllText(Path);
                   string Section = new Regex(@"(\[" + sectionName + @"\]\r?\n.+?\r?\n)\[", RegexOptions.Singleline | RegexOptions.Multiline).Match(read_file).Groups[1].Value;
                   //string Section2 = new Regex(@"(\[" + sectionName + @"\]\r?\n.+?\r?\n)[\[|$]",  RegexOptions.Singleline).Match(read_file).Groups[1].Value;
                   //Console.WriteLine("Sec2="+Section2); 
                   try
                   {
                       if (Section == "") Section = new Regex(@"(\[" + sectionName + @"\]\r?\n.+?\r?\n)$", RegexOptions.Singleline).Match(read_file).Groups[1].Value; 
                       File.WriteAllText(Path, read_file.Replace(Section, ""));

                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine("ex del sec = "+ex);
                       string read_log_file = System.IO.Path.GetTempPath() + $"tmp_log.log";
                       string read_log = "";
                       if (File.Exists(read_log_file))
                       {
                           read_log = File.ReadAllText(read_log_file);
                       }
                       read_log += "ex del sect: " + ex + "\r\n";
                       TextWriter tw = new StreamWriter(read_log_file);
                       tw.Write(read_log);
                       tw.Close();
                       DeleteSection(sectionName);
                   }
                   /*string[] delit = new string[255];
                   int i = -1;
                   foreach (SectionPair sectionPair in keyPairs.Keys) {
                       if (sectionPair.Section==sectionName) 
                       { 
                           i++;
                           delit[i]= sectionPair.Key;
                       }
                   }
                   foreach (string del in delit) {
                       DeleteKey(sectionName, del);
                   }*/

        //SaveSettings();
        //     IniParser();
        // }

        /// <summary>
        /// Save settings to new file.
        /// </summary>

        public void SaveSettings()
        {
            lock (keyPairs)
            {
                ArrayList sections = new ArrayList();
                string tmpValue;
                string strToSave = "";
                Hashtable tmpPairs = keyPairs;

                try
                {


                    foreach (SectionPair sectionPair in tmpPairs.Keys)
                    {
                        if (!sections.Contains(sectionPair.Section))
                            sections.Add(sectionPair.Section);
                    }
                    sections.Sort(new CompareSection());

                    foreach (string section in sections)
                    {
                        strToSave += ("[" + section + "]\r\n");
                        //Array.Sort(SectionPair);
                        //Array list;
                        //keyPairs.Keys.CopyTo(Array list, 0);
                        //keyPairs.Keys.
                        // foreach (SectionPair sectionPair in list)
                        foreach (SectionPair sectionPair in tmpPairs.Keys)
                        {

                            if (sectionPair.Section == section)
                            {
                                tmpValue = (string)tmpPairs[sectionPair];

                                if (tmpValue != null)
                                    tmpValue = "=" + tmpValue;
                                strToSave += (sectionPair.Key + tmpValue + "\r\n");
                            }
                        }

                        strToSave += "\r\n";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ex ini save = " + ex);
                    string lofFile = System.IO.Path.GetTempPath() + $"tmp_log.log";
                    string read_file = "";
                    if (File.Exists(lofFile))
                    {
                        read_file = File.ReadAllText(lofFile);
                    }
                    read_file += "ex ini save: " + ex + "\r\n";
                    TextWriter tw = new StreamWriter(lofFile);
                    tw.Write(read_file);
                    tw.Close();
                    SaveSettings();
                }
                try
                {
                    /*  Stream myStream;
                      using (myStream = File.Open(Path, FileMode.Open, FileAccess.Write))
                      {
                          StreamWriter myWriter = new StreamWriter(myStream);
                          myWriter.Write(strToSave);
                          Console.WriteLine(strToSave);
                      }*/
                    if (File.Exists(Path))
                    {
                        if (File.Exists(Path + ".bak"))
                        {
                            File.Replace(Path + ".bak", Path, Path + "._");
                        }
                        else
                        {
                            File.Move(Path, Path + ".bak");
                        }
                    }
                    TextWriter tw = new StreamWriter(Path);
                    tw.Write(strToSave);
                    tw.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ex saveSett = " + ex);
                    string lofFile = System.IO.Path.GetTempPath() + $"tmp_log.log";
                    string read_file = "";
                    if (File.Exists(lofFile))
                    {
                        read_file = File.ReadAllText(lofFile);
                    }
                    read_file += "error save sett: " + ex + "\r\n";
                    TextWriter tw = new StreamWriter(lofFile);
                    tw.Write(read_file);
                    tw.Close();
                    SaveSettings();
                }
            }
        }


        public bool KeyExists(string Section, string Key)
        {
            if(Read(Section, Key)==null) { return false; }
            return Read(Section, Key).Length > 0;
        }

        public void SortIni()
        {
            try
            {
                string read_file = File.ReadAllText(Path);
                int i = 0;
                string new_ini="";
                string global = new Regex(@"(\[GLOBAL\].+)", RegexOptions.Singleline | RegexOptions.Multiline).Match(read_file).Groups[1].Value;
                
                foreach (Match match in Regex.Matches(read_file, @"(USER_\d+\])(.+?)\[", RegexOptions.Singleline | RegexOptions.Multiline))
                {
                    i++;
                    new_ini += "[USER_"+i.ToString()+"]"+match.Groups[2].Value;
                    //Console.WriteLine("меняем USER_"+match.Groups[1].Value+" на USER_"+i);
                }
                new_ini+= global;
                TextWriter tw = new StreamWriter(Path);
                tw.Write(new_ini);
                tw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex sort sec = " + ex);
                throw ex;
            }
        }
    }
}
