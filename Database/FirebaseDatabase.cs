﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EcoHunt.Database
{
    public class FirebaseDatabase : DatabaseQueries
    {
        public static NamesValues[] GetAllPictureNames()
        {
            CheckForConnection();
            var response = GetData("Names");
            object json = JsonConvert.DeserializeObject(response.Body);

            List<NamesValues> patchNotes = new List<NamesValues>();
            foreach (JToken item in ((JToken)(json)).Children())
            {
                var newPatchNote = item.ToObject<NamesValues>();
                patchNotes.Add(newPatchNote);
            }

            for(int x = patchNotes.Count - 1; x >= 0; x--)
            {
                if (patchNotes[x] == null)
                    patchNotes.RemoveAt(x);
            }

            return patchNotes.ToArray();
        }
        public static void DeletePicture(string pictureName)
        {
            CheckForConnection();
            var allNames = GetAllPictureNames();
            for(int x = 0; x < allNames.Length; x++)
            {
                if(allNames[x].name == pictureName)
                {
                    DeletePicture(allNames[x].ID);
                    break;
                }
            }
        }
        public static void DeletePicture(int pictureIndex)
        {
            CheckForConnection();
            int currentNum = GetNumberOfNames();

            DeleteData("Names/" + pictureIndex);

            if (currentNum == pictureIndex)
                SetNumberOfNames(GetAllPictureNames().Last().ID);
        }

        public static int GetNumberOfNames()
        {
            CheckForConnection();
            return Convert.ToInt32(client.Get("NumberOfNames").Body);
        }
        private static void SetNumberOfNames(int number)
        {
            CheckForConnection();
            InsertData("NumberOfNames", number);
        }
        public static void AddUrlToPicture(string pictureName, string url)
        {
            var allNames = GetAllPictureNames();
            for(int x = 0; x < allNames.Length; x++)
            {
                if (allNames[x].name == pictureName)
                {
                    allNames[x].url = url;
                    //DeletePicture(allNames[x].name);
                    //AddPicture(allNames[x].name, allNames[x].ID, url);
                    UpdateData("Names/" + allNames[x].ID, allNames[x]);
                }
            }
        }
        public static void AddUrlsToPicturesWithoutUrls()
        {
            var allNames = GetAllPictureNames();
            for(int x = 0; x < allNames.Length; x++)
            {
                if (allNames[x].url == null || String.IsNullOrWhiteSpace(allNames[x].url) == true)
                {
                    string url = FirebaseCloudStorage.GetUrlForPhoto(allNames[x].name + ".jpg");
                    AddUrlToPicture(allNames[x].name, url);
                }
            }
        }
        public static void AddPicture(string picName)
        {
            CheckForConnection();
            int newID = GetNumberOfNames() + 1;

            NamesValues stuffToInput = new NamesValues { ID = newID, name = picName };

            InsertData("Names/" + stuffToInput.ID, stuffToInput);
            SetNumberOfNames(newID);
        }
        private static void AddPicture(string picName, int id, string Url)
        {
            CheckForConnection();
            NamesValues stuffToInput = new NamesValues { ID = id, url = Url, name = picName };
            InsertData("Names/" + stuffToInput.ID, stuffToInput);
        }
    }
    public class NamesValues
    {
        public string name;
        public int ID;
        public string url;
    }
}