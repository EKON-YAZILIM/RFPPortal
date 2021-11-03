﻿using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Models.Constants;
using RFPPortalWebsite.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Methods
{
    /// <summary>
    ///  Bidding methods bussiness layer
    /// </summary>
    public class BidMethods
    {
        /// <summary>
        ///  Post RFP Bid to database
        /// </summary>
        /// <param name="model">RfpBid model</param>
        /// <returns></returns>
        public static RfpBid SubmitBid(RfpBid model)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfp = db.Rfps.Find(model.RfpID);
                   
                    //Post bid to database
                    model.CreateDate = DateTime.Now;
                    db.RfpBids.Add(model);
                    db.SaveChanges();

                    //Logging
                    Program.monitizer.AddUserLog(model.UserId, Models.Constants.Enums.UserLogType.Auth, "User post bid successful. Bid: "+ Utility.Serializers.SerializeJson(model));

                    return model;
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return new RfpBid();
            }
        }

        /// <summary>
        ///  Delete RFP Bid record from database
        /// </summary>
        /// <param name="rfpbidid">RfpBid identity</param>
        /// <returns></returns>
        public static bool DeleteBid(int RfpBidID)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfpbid = db.RfpBids.Find(RfpBidID);

                    //Delete bid from database
                    db.RfpBids.Remove(rfpbid);
                    db.SaveChanges();

                    //Logging
                    Program.monitizer.AddUserLog(rfpbid.UserId, Models.Constants.Enums.UserLogType.Auth, "User delete bid successful. Bid: " + Utility.Serializers.SerializeJson(rfpbid));

                    return true;
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return false;
            }
        }

        /// <summary>
        ///  Choose winning bid from Rfp Bids with RfpBidID
        /// </summary>
        /// <param name="rfpbidid">Identity of the RfpBid</param>
        /// <returns></returns>
        public static bool ChooseWinningBid(RfpBid rfpbid)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfp = db.Rfps.Find(rfpbid.RfpID);

                    //Rfp winner bid database update
                    //rfp.WinnerRfpBidID = rfpbid.RfpBidID;
                    rfp.Status = Enums.RfpStatusTypes.Completed.ToString();

                    db.Entry(rfp).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    db.SaveChanges();

                    //Logging
                    Program.monitizer.AddUserLog(rfpbid.UserId, Models.Constants.Enums.UserLogType.Auth, "Admin choose winning bid successful. BidID: " + rfpbid.RfpBidID);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return false;
            }
        }

        /// <summary>
        ///  Edits RFP Bid to database
        /// </summary>
        /// <param name="model">RfpBid model</param>
        /// <returns></returns>
        public static RfpBid EditBid(RfpBid model)
        {
            try
            {
                using (rfpdb_context db = new rfpdb_context())
                {
                    var rfp = db.RfpBids.SingleOrDefault(x => x.RfpBidID == model.RfpBidID);

                    //Post edited bid to database
                    rfp.Amount = model.Amount;
                    rfp.Note = model.Note;
                    rfp.Time = model.Time;
                   
                    db.SaveChanges();

                    //Logging
                    Program.monitizer.AddUserLog(model.UserId, Models.Constants.Enums.UserLogType.Auth, "Edit bid successful. Bid: " + Utility.Serializers.SerializeJson(model));

                    return model;
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return new RfpBid();
            }
        }
    }
}
