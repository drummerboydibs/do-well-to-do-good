namespace DoWellToDoGood.Services;

/// <summary>
/// Public client configuration. The publishable key is designed to ship in
/// client code — every data path is protected by Row-Level Security, and all
/// entry content is end-to-end encrypted before it ever leaves the browser.
/// The service_role key must NEVER appear anywhere in this repository.
/// </summary>
public static class SupabaseConfig
{
    public const string Url = "https://vorzyrvkvdvkktdcwahl.supabase.co";
    public const string PublishableKey = "sb_publishable__XsvlnDT0Me6O1zNkr_dAQ_FMgowiZJ";
}
