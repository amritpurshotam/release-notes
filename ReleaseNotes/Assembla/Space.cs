namespace ReleaseNotes
{
    public class Space
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string wiki_name { get; set; }
        public int public_permissions { get; set; }
        public int team_permissions { get; set; }
        public int watcher_permissions { get; set; }
        public bool share_permissions { get; set; }
        public int team_tab_role { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string default_showpage { get; set; }
        public string tabs_order { get; set; }
        public string parent_id { get; set; }
        public bool restricted { get; set; }
        public object restricted_date { get; set; }
        public string commercial_from { get; set; }
        public object banner { get; set; }
        public object banner_height { get; set; }
        public string banner_text { get; set; }
        public object banner_link { get; set; }
        public string style { get; set; }
        public int status { get; set; }
        public bool approved { get; set; }
        public bool is_manager { get; set; }
        public bool is_volunteer { get; set; }
        public bool is_commercial { get; set; }
        public bool can_join { get; set; }
        public bool can_apply { get; set; }
        public object last_payer_changed_at { get; set; }
    }
}
