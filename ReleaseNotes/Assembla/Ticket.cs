namespace ReleaseNotes
{
    public class Ticket
    {
        public int id { get; set; }
        public int number { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public int priority { get; set; }
        public object completed_date { get; set; }
        public object component_id { get; set; }
        public string created_on { get; set; }
        public int permission_type { get; set; }
        public int importance { get; set; }
        public bool is_story { get; set; }
        public int milestone_id { get; set; }
        public string notification_list { get; set; }
        public string space_id { get; set; }
        public int state { get; set; }
        public string status { get; set; }
        public int story_importance { get; set; }
        public string updated_at { get; set; }
        public int working_hours { get; set; }
        public int estimate { get; set; }
        public int total_estimate { get; set; }
        public int total_invested_hours { get; set; }
        public int total_working_hours { get; set; }
        public string assigned_to_id { get; set; }
        public string reporter_id { get; set; }
        public CustomFields custom_fields { get; set; }
        public int hierarchy_type { get; set; }
        public object due_date { get; set; }
        public string space_name { get; set; }
    }
}
