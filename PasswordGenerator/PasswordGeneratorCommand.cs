using System;
using System.Management.Automation;

namespace PasswordGenerator
{
    [Cmdlet(VerbsCommon.Get, "Password")]
    public class GetGeneratedPasswordCommand : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [ValidateRange(6, 30)]
        public Int32 PwLength { get; set; }

        [Parameter(Mandatory = true)]

        public Int32 Amount { get; set; }

        [Parameter(Position = 1)]
        public SwitchParameter IncludeSpecial
        {
            get { return IncludeSpecial; }
            set { IncludeSpecial = value; }
        }

        [Parameter(Position = 2)]
        public SwitchParameter IncludeNumeric
        {
            get { return IncludeNumeric; }
            set { IncludeNumeric = value; }
        }


        protected override void ProcessRecord()
        {
            for (int i = 0; i < Amount; i++)
            {
                if (IncludeSpecial & IncludeNumeric)
                {
                    var pwd = new Password(PwLength).IncludeLowercase().IncludeUppercase().IncludeSpecial().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password);
                }

                if (IncludeSpecial & !IncludeNumeric)
                {
                    var pwd = new Password(PwLength).IncludeLowercase().IncludeUppercase().IncludeSpecial();
                    var password = pwd.Next();
                    WriteObject(password);
                }

                if (IncludeNumeric & !IncludeSpecial)
                {
                    var pwd = new Password(PwLength).IncludeLowercase().IncludeUppercase().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password);
                }

                if (!IncludeNumeric & !IncludeSpecial)
                {
                    var pwd = new Password(PwLength).IncludeLowercase().IncludeUppercase();
                    var password = pwd.Next();
                    WriteObject(password);
                }

            }

        }

    }
}