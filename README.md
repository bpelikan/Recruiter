# Recruiter
[![Build Status](https://bpelikan.visualstudio.com/Recruiter/_apis/build/status/Recruiter-master-CI)](https://bpelikan.visualstudio.com/Recruiter/_build/latest?definitionId=2)
* **Web App**, 
* **SQL database**
## Setting up
What you need:
* **Web App** with:
  * `Application settings`:
    * `"ConnectionStrings:DefaultConnection": "{connection_string_to_sql_database}"`
    * `"AdminEmail": "{email_for_admin_account}"`
    * `"AdminPassword": "{password_for_admin_account}"`
    * `"SendGridKey": "{SendGrid_API_Key}"`
    * `"blobServiceEndpoint": "{primary_blob_service_endpoint}"`
    * `"blobAccountName": "{storage_account_name}"`
    * `"blobKeyValue": "{blob_access_key}"`

* **SQL database**
