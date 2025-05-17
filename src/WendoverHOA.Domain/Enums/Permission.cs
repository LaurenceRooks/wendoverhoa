namespace WendoverHOA.Domain.Enums;

/// <summary>
/// Defines the permissions available in the system for role-based access control
/// </summary>
public enum Permission
{
    // General permissions
    ViewPublicContent = 100,
    
    // User management permissions
    ViewUsers = 200,
    CreateUser = 201,
    UpdateUser = 202,
    DeleteUser = 203,
    ManageUserRoles = 204,
    
    // Announcement permissions
    ViewAnnouncements = 300,
    CreateAnnouncement = 301,
    UpdateAnnouncement = 302,
    DeleteAnnouncement = 303,
    
    // Calendar permissions
    ViewCalendar = 400,
    CreateEvent = 401,
    UpdateEvent = 402,
    DeleteEvent = 403,
    
    // Document permissions
    ViewDocuments = 500,
    UploadDocument = 501,
    UpdateDocument = 502,
    DeleteDocument = 503,
    
    // Directory permissions
    ViewDirectory = 600,
    CreateDirectoryEntry = 601,
    UpdateDirectoryEntry = 602,
    DeleteDirectoryEntry = 603,
    
    // Board management permissions
    ViewBoardInfo = 700,
    ManageBoardMembers = 701,
    
    // Meeting minutes permissions
    ViewMeetingMinutes = 800,
    CreateMeetingMinutes = 801,
    UpdateMeetingMinutes = 802,
    DeleteMeetingMinutes = 803,
    
    // Feedback permissions
    ViewFeedback = 900,
    SubmitFeedback = 901,
    RespondToFeedback = 902,
    DeleteFeedback = 903,
    
    // Vendor permissions
    ViewVendors = 1000,
    SuggestVendor = 1001,
    ApproveVendor = 1002,
    DeleteVendor = 1003,
    
    // Financial permissions
    ViewFinancialInfo = 1100,
    ManageFinancialRecords = 1101,
    ProcessPayments = 1102,
    
    // System permissions
    ManageSystemSettings = 1200,
    ViewAuditLogs = 1201,
    ManageApiAccess = 1202
}
