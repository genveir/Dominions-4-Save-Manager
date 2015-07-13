using System;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections.Generic;

using System.Windows.Forms;

namespace Graphical_DomTurn
{
    class DomTurnSaver
    {
        private String sourcePath;

        private String savefile;
        private String gamefolder;

        private DateTime lastSave;

        public DomTurnSaver()
        {
            String appData = Environment.ExpandEnvironmentVariables("%AppData%");

            String saveLocation = Path.Combine("Dominions4", "savedgames");

            sourcePath = Path.Combine(appData, saveLocation);
        }

        public bool SetGame(String name)
        {
            bool succeeded = true;

            try
            {
                gamefolder = Path.Combine(sourcePath, name);

                savefile = Directory.GetFiles(gamefolder, "*.trn")[0];

                lastSave = File.GetLastWriteTime(savefile);
            }
            catch (Exception)
            {
                succeeded = false;
            }
            return succeeded;
        }

        public String[] getGames()
        {
            String[] games = Directory.GetDirectories(sourcePath);

            for (int n = 0; n < games.Length; n++)
            {
                games[n] = games[n].Replace(sourcePath + Path.DirectorySeparatorChar, "");
            }

            String[] noNewLords = new String[games.Length - 1];

            int i = 0;
            for (int n = 0; n < games.Length; n++)
            {
                if (games[n].Equals("newlords")) continue;
                else noNewLords[i++] = games[n];
            }

            return noNewLords;
        }

        public bool checkUpdate()
        {
            DateTime currentSave = File.GetLastWriteTime(savefile);

            if (currentSave != lastSave)
            {
                lastSave = currentSave;
                return true;
            }
            return false;
        }

        public void SetTo(Game source)
        {
            copyFiles(source.path, gamefolder);

            String dataFilePath = Path.Combine(gamefolder, "data.txt");
            if (File.Exists(dataFilePath)) File.Delete(dataFilePath);

            lastSave = File.GetLastWriteTime(savefile);
        }

        public Game saveGame(String currentPath)
        {
            Game newGame = new Game();

            String savePath = currentPath + "t";
            newGame.path = currentPath;
            int n = 0;

            while (Directory.Exists(newGame.path)) {
                newGame.path = savePath + n++;
            }
            Directory.CreateDirectory(newGame.path);

            copyFiles(gamefolder, newGame.path);

            newGame.name = "turn" + ((n == 0) ? "" : " (" + n + ")");

            StreamWriter writer = new StreamWriter(Path.Combine(currentPath, "data.txt"), true);
            writer.WriteLine(newGame.path);
            writer.Close();

            writer = new StreamWriter(Path.Combine(newGame.path, "data.txt"));
            writer.WriteLine(newGame.name);
            writer.Close();

            return newGame;
        }

        public Game getGame(String currentPath)
        {
            Game game = new Game();

            game.path = currentPath;

            if (!Directory.Exists(currentPath)) saveGame(currentPath);

            StreamReader reader = new StreamReader(Path.Combine(currentPath, "data.txt"));
            game.name = reader.ReadLine();
            reader.Close();

            return game;
        }

        public String[] getChildrenPaths(String currentPath)
        {
            StreamReader reader = new StreamReader(Path.Combine(currentPath, "data.txt"));
            reader.ReadLine(); // name

            List<String> children = new List<String>();
            while (!reader.EndOfStream)
            {
                children.Add(reader.ReadLine());
            }
            reader.Close();

            return children.ToArray();
        }

        private void copyFiles(String sourcePath, String targetPath)
        {
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

            String[] files = Directory.GetFiles(sourcePath);

            foreach (String file in files)
            {
                File.Copy(file, file.Replace(sourcePath, targetPath), true);
            }
        }

        public void Clear(String path)
        {
            System.Diagnostics.Debug.Print("clearing " + path);

            if (Directory.Exists(path))
            {
                System.Diagnostics.Debug.Print("clearing " + path);

                if (path.Contains("dom4turnsaver\\"))
                {
                    Directory.Delete(path, true);
                }
            }
        }

        public bool equalsCurrentGame(Game game)
        {
            String filetrn = Directory.GetFiles(game.path, "*.trn")[0];

            String currenttrn = Directory.GetFiles(gamefolder, "*.trn")[0];

            return doCompare(filetrn, currenttrn);
        }

        private bool doCompare(String file1, String file2)
        {
            bool sameFile = true;
            if (new FileInfo(file1).Length == new FileInfo(file2).Length)
            {
                StreamReader reader1 = new StreamReader(file1);
                StreamReader reader2 = new StreamReader(file2);

                while (!reader1.EndOfStream)
                {
                    if (reader2.EndOfStream)
                    {
                        sameFile = false;
                        break;
                    }

                    String read1 = reader1.ReadLine();
                    String read2 = reader2.ReadLine();

                    if (!read1.Equals(read2))
                    {
                        sameFile = false;
                        break;
                    }
                }
                reader1.Close();
                reader2.Close();
            }
            else sameFile = false;

            System.Diagnostics.Debug.Print(file1);
            System.Diagnostics.Debug.Print(file2);
            System.Diagnostics.Debug.Print("" + sameFile);

            return sameFile;
        }
    }
}
