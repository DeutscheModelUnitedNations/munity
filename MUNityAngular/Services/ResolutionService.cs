﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MUNityAngular.Util.Extenstions;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using MUNityAngular.DataHandlers.Database;
using MongoDB.Driver;

namespace MUNityAngular.Services
{
    public class ResolutionService : IDisposable
    {
        private const string resolution_table_name = "resolution";

        public IHubContext<Hubs.ResolutionHub, Hubs.ITypedResolutionHub> HubContext { get; set; }

        //private List<Models.ResolutionModel> resolutions = new List<Models.ResolutionModel>();

        private readonly IMongoCollection<Models.ResolutionModel> _resolutions;

        private Timer _saveTimer;

        private Stack<Models.ResolutionModel> _saveStack = new Stack<Models.ResolutionModel>();

        public Models.ResolutionModel CreateResolution(bool isPublicReadable = false, bool isPublicWriteable = false, string userid = "anon")
        {
            var resolution = new Models.ResolutionModel();
            this._resolutions.InsertOne(resolution);
            //resolutions.Add(resolution);
            //Add To database
            SaveResolutionInDatabase(resolution, isPublicReadable, isPublicWriteable, userid);

            //Create file
            return resolution;
        }

        private string Save(Models.ResolutionModel resolution)
        {
            var resolutionDirectory = DataHandlers.Database.SettingsHandler.GetResolutionDir;
            var filePath = System.IO.Path.Combine(resolutionDirectory, resolution.ID + ".json");

            if (!System.IO.File.Exists(filePath))
            {
                var stream = System.IO.File.Create(filePath);
                stream.Close();
            }
            System.IO.File.WriteAllText(filePath, resolution.ToJson());
            if (HubContext != null)
                HubContext.Clients.Group(resolution.ID).ResolutionSaved(DateTime.Now);
            return filePath;
        }

        //private void SaveStack(object state)
        //{
        //    Console.WriteLine(DateTime.Now + " Start Saving stack");
        //    int total = _saveStack.Count;
        //    int stackCount = _saveStack.Count;
        //    while (stackCount > 0)
        //    {
        //        var resolution = _saveStack.Pop();
        //        Save(resolution);
        //        stackCount = _saveStack.Count;
        //    }
        //    Console.WriteLine("Stack has been Saved total of " + total + " elements.");
        //}

        //private Task StartSaveTask(CancellationToken stoppingToken)
        //{
        //    _saveTimer = new Timer(SaveStack, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        //    return Task.CompletedTask;
        //}

        //private Task StopSaveTask(CancellationToken stoppingToken)
        //{
        //    _saveTimer?.Change(Timeout.Infinite, 0);

        //    return Task.CompletedTask;
        //}

        public void RequestSave(Models.ResolutionModel resolution)
        {
            
            this._resolutions.ReplaceOne(n => n.ID == resolution.ID, resolution);

            //Old Saving with the Stack!
            /*
            if (resolution == null)
            {
                return;
            }

            if (!_saveStack.Contains(resolution))
            {
                Console.WriteLine("Pushed Resolution to save stack " + resolution.ID);
                _saveStack.Push(resolution);
            }
            */
        }

        public Models.ResolutionModel GetResolution(string id)
        {
            
            var resolution = this._resolutions.Find(n => n.ID == id).FirstOrDefault();
            //if (resolution == null)
            //{
            //    var resolutionDirectory = DataHandlers.Database.SettingsHandler.GetResolutionDir;
            //    var filePath = System.IO.Path.Combine(resolutionDirectory, id + ".json");
            //    if (System.IO.File.Exists(filePath))
            //    {
            //        resolution = GetResolutionFromJson(System.IO.File.ReadAllText(filePath));
            //        resolutions.Add(resolution);
            //    }
            //    else
            //    {
            //        return null;
            //    }
            //}
            return resolution;
        }



        public (string id, bool publicRead, bool publicWrite, bool publicAmendment) GetResolutionInfoForPublicId(string publicid)
        {
            string id = string.Empty;
            bool isPublicRead = false;
            bool isPublicWrite = false;
            bool isOnlineAmendment = false;
            using (var connection = Connector.Connection)
            {
                var cmdStr = "SELECT * FROM resolution WHERE onlinecode=@onlineid";
                connection.Open();
                var cmd = new MySqlCommand(cmdStr, connection);
                cmd.Parameters.AddWithValue("@onlineid", publicid);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id = reader.GetString("id");
                        isPublicRead = reader.GetBoolean("ispublicread");
                        isPublicWrite = reader.GetBoolean("ispublicwrite");
                        isOnlineAmendment = reader.GetBoolean("allowamendments");
                    }
                }
            }

            return (id, isPublicRead, isPublicWrite, isOnlineAmendment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Touple with infos publicid is null when the resolution can not be found!</returns>
        public Models.ResolutionAdvancedInfoModel GetResolutionInfoForId(string id)
        {
            Models.ResolutionAdvancedInfoModel model = null;
            using (var connection = Connector.Connection)
            {
                var cmdStr = "SELECT * FROM resolution WHERE id=@id";
                connection.Open();
                var cmd = new MySqlCommand(cmdStr, connection);
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        model = null;
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            model = new Models.ResolutionAdvancedInfoModel();
                            model.ID = id;
                            model.Name = reader.GetString("name");
                            model.OnlineCode = reader.GetString("onlinecode");
                            model.CreationDate = reader.GetDateTime("creationdate");
                            model.LastChangedDate = reader.GetDateTime("lastchangeddate");
                            model.PublicRead = reader.GetBoolean("ispublicread");
                            model.PublicWrite = reader.GetBoolean("ispublicwrite");
                            model.PublicAmendment = reader.GetBoolean("allowamendments");
                        }
                    }

                    
                }
            }

            return model;
        }

        /// <summary>
        /// Activates the Public-Read-Mode and returns the Public Id of
        /// the resolution.
        /// </summary>
        /// <param name="resolutionid"></param>
        /// <returns></returns>
        public string ActivatePublicReadMode(string resolutionid)
        {
            string publicid = GetPublicIdForResolution(resolutionid);

            //Suche nach der Resolution in der Datenbank und schaue ob diese bereits eine PublicId hat
            using (var connection = Connector.Connection)
            {
                connection.Open();
                
                if (string.IsNullOrWhiteSpace(publicid))
                {
                    publicid = GeneratePublicId();
                }
                //Updaten der Resolution:
                var updateCommandStr = "UPDATE resolution SET onlinecode=@onlinecode, ispublicread=1";
                var updateCommand = new MySqlCommand(updateCommandStr, connection);
                updateCommand.Parameters.AddWithValue("@onlinecode", publicid);
                updateCommand.ExecuteNonQuery();
            }
            return publicid;
        }

        public string GeneratePublicId()
        {
            string id = new Random().Next(10000000, 99999999).ToString();
            bool containsId = true;
            using (var connection = Connector.Connection)
            {
                var cmdStr = "SELECT COUNT(*) FROM resolution WHERE onlinecode=@code";
                var cmd = new MySqlCommand(cmdStr, connection);
                cmd.Parameters.AddWithValue("@code", id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        containsId = (reader.GetInt16(0) > 0);
                    }
                }
            }
            return id;
        }

        public string GetPublicIdForResolution(string resolutionid)
        {
            string publicid = null;
            using (var connection = Connector.Connection)
            {
                connection.Open();
                var getOnlineIdCommand = "SELECT onlinecode FROM resolution WHERE id=@resoid";
                var cmd = new MySqlCommand(getOnlineIdCommand, connection);
                cmd.Parameters.AddWithValue("@resoid", resolutionid);
                using (var idreader = cmd.ExecuteReader())
                {
                    while (idreader.Read())
                    {
                        publicid = idreader.GetString(0);
                    }
                }
            }
            return publicid;
        }

        public Models.ResolutionModel GetResolutionFromJson(string json)
        {
            var resolution = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.ResolutionModel>(json);
            return resolution;
        }

        public string GetResolutionJsonFromFile(string id)
        {
            var resolutionDirectory = DataHandlers.Database.SettingsHandler.GetResolutionDir;
            var filePath = System.IO.Path.Combine(resolutionDirectory, id + ".json");
            if (System.IO.File.Exists(filePath))
            {
                return System.IO.File.ReadAllText(filePath);
            }
            else
            {
                return null;
            }
        }

        
        private bool SaveResolutionInDatabase(Models.ResolutionModel resolution, bool pread, bool pwrite, string userid = "anon")
        {
            using (var connection = Connector.Connection)
            {
                var cmdStr = "INSERT INTO "+ resolution_table_name + " (id, name, user, creationdate, lastchangeddate, onlinecode,ispublicread,ispublicwrite) VALUES (" +
                    "@id, @name, @user, @creationdate, @lastchangeddate, @onlinecode, @pread, @pwrite);";
                connection.Open();
                var cmd = new MySqlCommand(cmdStr, connection);
                cmd.Parameters.AddWithValue("@id", resolution.ID);
                cmd.Parameters.AddWithValue("@name", resolution.Topic);
                cmd.Parameters.AddWithValue("@user", userid);
                cmd.Parameters.AddWithValue("@creationdate", DateTime.Now);
                cmd.Parameters.AddWithValue("@lastchangeddate", DateTime.Now);
                cmd.Parameters.AddWithValue("@onlinecode", new Random().Next(10000000, 99999999));
                cmd.Parameters.AddWithValue("@pread", pread);
                cmd.Parameters.AddWithValue("@pwrite", pwrite);
                cmd.ExecuteNonQuery();
                return true;
            }
        }

        public void Dispose()
        {
            _saveTimer?.Dispose();
        }

        

        /// <summary>
        /// Returns a List of resolutions where the User is the creator/owner
        /// </summary>
        /// <param name="userid"></param>
        /// <returns>a touple with the first value as the id, and the second value is the name</returns>
        internal List<Models.ResolutionInformationModel> GetResolutionsOfUser(string userid)
        {
            var cmdStr = "SELECT id, name FROM " + resolution_table_name + " WHERE user=@userid";
            var list = new List<Models.ResolutionInformationModel>();
            using (var connection = Connector.Connection)
            {
                connection.Open();
                var cmd = new MySqlCommand(cmdStr, connection);
                cmd.Parameters.AddWithValue("@userid", userid);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Models.ResolutionInformationModel() {ID = reader.GetString(0), Name = reader.GetString(1)});
                    }
                }
            }
            return list;
        }

        internal void UpdateResolutionName(string resolutionid, string newtitle)
        {
            var cmdStr = "UPDATE " + resolution_table_name + " SET name = @newname WHERE id=@id";
            using (var connection = Connector.Connection)
            {
                connection.Open();
                var cmd = new MySqlCommand(cmdStr, connection);
                cmd.Parameters.AddWithValue("@newname", newtitle);
                cmd.Parameters.AddWithValue("@id", resolutionid);
                cmd.ExecuteNonQuery();
            }
        }

        public ResolutionService()
        {
            //New Saving in MongoDB
            var connectionString = "mongodb://localhost:27017";
            var databaseString = "MunityDb";
            var resolutionTableString = "Resolutions";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseString);

            this._resolutions = database.GetCollection<Models.ResolutionModel>(resolutionTableString);
        }
    }

    
}
