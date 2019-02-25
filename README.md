# Recruiter
[![Build Status](https://bpelikan.visualstudio.com/Recruiter/_apis/build/status/Recruiter-master-CI)](https://bpelikan.visualstudio.com/Recruiter/_build/latest?definitionId=2)

This project uses:
* **Web App**, 
* **SQL database**,
* **Storage account**
* **Application Insights**
* **SendGrid**
* [azure-functions-recruiter](https://github.com/bpelikan/azure-functions-recruiter "azure-functions-recruiter")
## Setting up on Azure
What is needed to deploy this project on Azure:

* **Web App** with:
  * `Application settings`:
    * `"ConnectionStrings:DefaultConnection": "{connection_string_to_sql_database}"`
    * `"APPINSIGHTS_INSTRUMENTATIONKEY": "{ApplicationInsights_InstrumentationKey}"`
    * `"AdminEmail": "{email_for_admin_account}"`
    * `"AdminPassword": "{password_for_admin_account}"`
    * `"SendGridKey": "{SendGrid_API_Key}"`
    * `"baseBlobUri": "{primary_blob_service_endpoint}"`
    * `"blobAccountName": "{storage_account_name}"`
    * `"blobKeyValue": "{blob_access_key}"`

* **SQL database**
* **Storage account**
* **Application Insights**
* **SendGrid account**
* **[azure-functions-recruiter](https://github.com/bpelikan/azure-functions-recruiter "azure-functions-recruiter")**
