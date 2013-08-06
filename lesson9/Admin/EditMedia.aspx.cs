﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace lesson9.Admin
{
    public partial class EditMedia : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnUploadMedia_Click(object sender, EventArgs e)
        {
          string location = "/Medias/" + fupMedia.FileName;
          fupMedia.SaveAs(AppDomain.CurrentDomain.BaseDirectory + location);
          BusinessRules.CMedia objPost = new BusinessRules.CMedia();
          objPost.Title = txtTitle.Text;
          objPost.Location = location;
          objPost.UserID = BusinessRules.CUser.getIDByName(HttpContext.Current.User.Identity.Name);
          objPost.save();
          Response.Redirect("/Media.aspx", true);
        }
    }
}