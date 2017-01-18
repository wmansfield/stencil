using Stencil.Common.Integration;
using Stencil.Data.Sql;
using Stencil.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Primary.Business.Direct.Implementation
{
    public partial class AssetBusiness
    {
        public List<Asset> GetVideosForProcessing(string encode_status, bool include_empty_status)
        {
            return base.ExecuteFunction("GetVideosForProcessing", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    var foundItems = (from a in db.dbAssets
                                      where a.type == (int)AssetType.Video
                                      && a.encode_required == true
                                      &&
                                      (
                                         a.encode_status == encode_status
                                         || (include_empty_status && (a.encode_status == null || a.encode_status == ""))
                                      )
                                      orderby a.created_utc ascending
                                      select a
                                     );

                    return foundItems.ToDomainModel();
                }
            });
        }
        public List<Asset> GetVideosForRetrying(int attemptsLessThan, DateTime minimumAttemptTime)
        {
            return base.ExecuteFunction("GetVideosForRetrying", delegate ()
            {

                using (var db = base.CreateSQLContext())
                {
                    var foundItems = (from a in db.dbAssets
                                      where a.type == (int)AssetType.Video
                                      && a.encode_required == true
                                      &&
                                      (
                                         a.encode_status == EncoderStatus.fail.ToString()
                                         || a.encode_status == EncoderStatus.raw.ToString()
                                      )
                                      &&
                                      (
                                        a.encode_attempts < attemptsLessThan
                                      )
                                      &&
                                      (
                                        a.encode_attempt_utc.Value <= minimumAttemptTime
                                      )
                                      orderby a.created_utc ascending
                                      select a
                                     );

                    return foundItems.ToDomainModel();
                }
            });
        }
        public List<Asset> GetVideosFailedProcessingAfter(int attemptsGreaterThanOrEqualTo, DateTime lastFailureAfter)
        {
            return base.ExecuteFunction("GetVideosFailedProcessingAfter", delegate ()
            {

                using (var db = base.CreateSQLContext())
                {
                    var foundItems = (from a in db.dbAssets
                                      where a.type == (int)AssetType.Video
                                      && a.encode_required == true
                                      &&
                                      (
                                         a.encode_status == EncoderStatus.fail.ToString()
                                         || a.encode_status == EncoderStatus.raw.ToString()
                                      )
                                      &&
                                      (
                                        a.encode_attempts >= attemptsGreaterThanOrEqualTo
                                      )
                                      &&
                                      (
                                        a.encode_attempt_utc.Value > lastFailureAfter
                                      )
                                      orderby a.created_utc ascending
                                      select a
                                     );

                    return foundItems.ToDomainModel();
                }
            });
        }

        public Asset UpdateEncodingInfo(Guid asset_id, string encode_identifier, bool is_processing, string processor_status, string processor_log, bool incrementEncodingAttempt)
        {
            return base.ExecuteFunction("UpdateEncodingInfo", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    dbAsset found = (from a in db.dbAssets
                                     where a.asset_id == asset_id
                                     select a).FirstOrDefault();

                    if (found != null)
                    {
                        found.encode_processing = is_processing;
                        found.encode_identifier = encode_identifier;
                        found.encode_status = processor_status;
                        found.encode_log = processor_log;
                        if (incrementEncodingAttempt)
                        {
                            found.encode_attempt_utc = DateTime.UtcNow;
                            found.encode_attempts = found.encode_attempts + 1;
                        }
                        db.SaveChanges();

                        return found.ToDomainModel();
                    }
                    return null;
                }
            });
        }
        public List<Asset> GetPhotosForProcessing(string resize_status, bool include_empty_status, int maxRetries)
        {
            return base.ExecuteFunction("GetPhotosForProcessing", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    var foundItems = (from a in db.dbAssets
                                      where a.type == (int)AssetType.Image
                                      && a.resize_required == true
                                      && a.resize_attempts < maxRetries
                                      &&
                                      (
                                         a.resize_status == resize_status
                                         || (include_empty_status && (a.resize_status == null || a.resize_status == ""))
                                      )
                                      orderby a.created_utc ascending
                                      select a
                                     );

                    return foundItems.ToDomainModel();
                }
            });
        }

        public void UpdateResizeAttemptInfo(Guid asset_id, int attempt_count, DateTime attempt_utc, string resize_log)
        {
            base.ExecuteMethod("UpdateResizeAttemptInfo", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    var found = (from a in db.dbAssets
                                 where a.asset_id == asset_id
                                 select a).FirstOrDefault();

                    if (found != null)
                    {
                        found.resize_attempts = attempt_count;
                        found.resize_attempt_utc = attempt_utc;
                        found.resize_log = resize_log;
                        db.SaveChanges();
                    }
                }
            });
        }
        public void UpdateResizeInfo(Guid asset_id, bool is_processing, string processor_status, string processor_log)
        {
            base.ExecuteMethod("UpdateResizeInfo", delegate ()
            {
                using (var db = base.CreateSQLContext())
                {
                    var found = (from a in db.dbAssets
                                 where a.asset_id == asset_id
                                 select a).FirstOrDefault();

                    if (found != null)
                    {
                        found.resize_processing = is_processing;
                        found.resize_status = processor_status;
                        found.resize_log = processor_log;
                        db.SaveChanges();
                    }
                }
            });
        }
    }
}
