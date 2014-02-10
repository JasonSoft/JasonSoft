// Copyright 2008 JasonSoft - http://www.jasonsoft.net/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


using System.Collections.Generic;

namespace JasonSoft.Web.ViewState
{
    /// <summary>
    /// Provides the ability to save viewstate into session. The session will double as a viewstate server.
    /// </summary>
    public class ViewStateSessionStorage : IViewstateStorage
    {
        private System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object, string>> StorageList
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["ViewStateSessionStorage"] == null)
                {
                    System.Web.HttpContext.Current.Session["ViewStateSessionStorage"] =
                        new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object, string>>();
                }
                return
                    ((System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object, string>>)
                     System.Web.HttpContext.Current.Session["ViewStateSessionStorage"]);
            }
        }

        #region IViewstateStorage Members

        public void Add(KeyValuePair<object, string> viewstateItem)
        {
            StorageList.Add(viewstateItem);
        }

        public KeyValuePair<object, string> Get(object key)
        {
            System.Collections.Generic.KeyValuePair<object, string> retItem = new KeyValuePair<object, string>();
            foreach (System.Collections.Generic.KeyValuePair<object, string> item in this.StorageList)
            {
                if (item.Key.ToString().ToLower().Equals(key.ToString().ToLower()))
                {
                    retItem = item;
                }
            }

            //Remove(retItem.Key);
            // Can't more
            if(StorageList.Count > 150) StorageList.RemoveAt(0);
            return retItem;
        }

        public void Remove(object key)
        {
            System.Collections.Generic.KeyValuePair<object, string> itemToRemove =
                new System.Collections.Generic.KeyValuePair<object, string>();
            foreach (System.Collections.Generic.KeyValuePair<object, string> item in this.StorageList)
            {
                if (item.Key == key)
                {
                    itemToRemove = item;
                }
            }
            this.StorageList.Remove(itemToRemove);
        }

        #endregion
    }
}