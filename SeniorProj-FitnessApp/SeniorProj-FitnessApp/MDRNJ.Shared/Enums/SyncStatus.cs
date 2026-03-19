using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.Enums
{
    public enum SyncStatus
    {
        Pending = 0,  // saved locally, not yet sent to server
        Syncing = 1,  // currently being uploaded
        Synced = 2,  // successfully saved to server
        Failed = 3   // sync attempt failed, will retry
    }
}
