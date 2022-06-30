using System;
using System.Management.Automation;
using PasswordGenerator;

namespace PasswordGenerator
{
    [Cmdlet(VerbsCommon.Get,"Password")]
    public class GetGeneratedPasswordCommand : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateRange(6,30)]
        public Int32 PwLength { get; set; }

        [Parameter(Mandatory = true)]
        
        public Int32 Amount { get; set;}

        protected override void ProcessRecord()
        {
            for (int i = 0; i < Amount; i++)
            {
                var pwd = new Password(PwLength).IncludeLowercase().IncludeUppercase().IncludeSpecial().IncludeNumeric();
                var password = pwd.Next();
                WriteObject(password);   
            }
            
        }

    }
}

