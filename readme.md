# What Is My Ip

A 64bit Windows Service that communicates with an external host to retrieve the external ip of the network the service operates on.
Useful for services that are running on a network with a dynamic external ip.

## Features:

- Updates IIS FTP External Firewall Address if the external ip has changed.
- Notifies an admin of the ipvia email so they may update external DNS settings if required.

### Prerequisites

- [Microsoft .Net 4.7.1+] (https://www.microsoft.com/en-us/download/details.aspx?id=56116)

#### Modules
- [IIS 8.5+] (https://docs.microsoft.com/en-us/iis/install/installing-iis-85/installing-iis-85-on-windows-server-2012-r2)

## Installation

To install this service:

1. Save the exe into a folder from where you'd like it to run from. Eg. "C:\WhatIsMyIp"
1. Open a command prompt, as an Administrator, and travel to the Microsoft .Net 4 folder at "C:\Windows\Microsoft.NET\Framework64\v4.0.30319".
   1. "cd C:\Windows\Microsoft.NET\\**_Framework64_**\\v4.0.30319"
      - Ensure you're in the Framework64 folder. This is a 64bit service.
2. Type the following into the command prompt:
   1. InstallUtil.exe "C:\WhatIsMyIp\WhatIsMyIp.exe"
3. The following prompts will appear and the information gathered stored in the Registry at "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WhatIsMyIp"
   1. Service Host: Enter the web address of the service that will respond to a webrequest with the external ip.
       - Expected result is a single string containing the external ip address. Eg. "111.111.111.111"
       - Modify parsing if another service provides more information or in another format.
   2. Email To: Enter the email address of the person who will be notified when the external ip has changed.
   3. Email From: Enter the email address that will be sending the email.
   4. Email Host: Enter the SMTP server's address.
   5. Email Port: Enter the SMTP server's email port.
   6. Email SSL Enabled: Enter "true" or "false" to enable or disable SSL requirement for emails. Must match SMTP requirements.
   7. Log File Path: Enter the file path for the log files to be saved. Eg. "C:\WhatIsMyIp\Logs"
   8. Interval: Enter the amount of time between calls to the external Service Host in milliseconds. Eg. "500000" is 5 minutes.
4. Confirm the settings or press a number from 1-8 to re-enter information for that setting.
5. Once confimed a propmt will appear asking for credentials. These credentials must have rights to read/write from/to the Registry and be able to read wherever the exe is stored. The account must also be able to modify IIS server settings on the local server.
6. Service should now be up and running. To test, force an ip change for your external ip. Check logs and email for confirmation of updates.
