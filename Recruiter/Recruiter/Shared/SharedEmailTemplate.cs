using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Shared
{
    public static class SharedEmailTemplate
    {
        public static string GetEmailTemplate(string title, string content)
        {
            return $@"
                    <!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
                    <html xmlns='http://www.w3.org/1999/xhtml'>
                    <head>
	                    <meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />
	                    <title>Recruiter</title>
	                    <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                    </head>                
                    <body style='margin: 0; padding: 0;'>
                    <table align='center' border='0' cellpadding='0' cellspacing='0' width='600' style='border-collapse: collapse; border: 1px solid #cccccc;'>
	                    <tr>
		                    <td bgcolor='#ffffff' style='padding: 40px 30px 40px 30px;'>

                                <table border='0' cellpadding='0' cellspacing='0' width='100%'>
	                                <tr>
		                                <td style='color: #153643; font-family: Arial, sans-serif; font-size: 24px;'>
			                                <b>
                                                {title}
                                            </b>
		                                </td>
	                                </tr>
	                                <tr>
		                                <td style='padding: 20px 0 30px 0; color: #153643; font-family: Arial, sans-serif; font-size: 16px; line-height: 20px;'>
                                            {content}
		                                </td>
	                                </tr>
                                </table>
		                    </td>
        
	                    </tr>
	                    <tr>
		                    <td bgcolor='#0254E6' style='padding: 30px 30px 30px 30px;'>
			                    <table border='0' cellpadding='0' cellspacing='0' width='100%'>
				                    <tr>
					                    <td width='75%' style='color: #ffffff; font-family: Arial, sans-serif; font-size: 14px;'>
						                    &reg; <a href='https://recruiterbp.azurewebsites.net/' style='color: #ffffff;'>Recruiter {DateTime.UtcNow.Year}</a>  
						
						                    <br/>
					                    </td>
					                    <td align='right'>
						                    <table border='0' cellpadding='0' cellspacing='0'>
							                    <tr>
								                    <td>
									                    <a href='https://recruiterbp.azurewebsites.net/'>
										                    <img src='https://recruiterbpstorage.blob.core.windows.net/static/website.png' alt='Recruiter website' width='38' height='38' style='display: block;' border='0' />
									                    </a>
								                    </td>
							                    </tr>
						                    </table>
					                    </td>
				                    </tr>
			                    </table>
		                    </td>
	                    </tr>
                    </table>
                    </body>
                    </html>
                    ";
        }
    }
}
