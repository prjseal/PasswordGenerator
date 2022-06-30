using System;
using System.Management.Automation;
using PasswordGenerator;

namespace PasswordGenerator
{
    [Cmdlet(VerbsCommon.Get,"Password")]
    public class GetGeneratedPasswordCommand : PSCmdlet
    {
        private int _pwLengthDefault = 14;
        [Parameter]
        [ValidateRange(6,30)]
        public Int32 PwLength
        {
            get
            {
                return _pwLengthDefault;
            }
            set
            {
                _pwLengthDefault = value;
            }
        }

        private int _amountDefault = 1;
        [Parameter]
        public Int32 Amount
        {
            get
            {
                return _amountDefault;
            }
            set
            {
                _amountDefault = value;
            }
        }

        [Parameter]
        public SwitchParameter Specials { get; set; }
        [Parameter]
        public SwitchParameter Numeric { get; set; }

        protected override void ProcessRecord()
        {
            for (int i = 0; i < Amount; i++)
            {
                if (Specials & Numeric)
                {
                    var pwd = new Password(PwLength).IncludeLowercase().IncludeUppercase().IncludeSpecial().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password); 
                }
                else if (Numeric)
                {
                    var pwd = new Password(PwLength).IncludeLowercase().IncludeUppercase().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password); 
                }
                else if (Specials)
                {
                    var pwd = new Password(PwLength).IncludeLowercase().IncludeUppercase().IncludeSpecial();
                    var password = pwd.Next();
                    WriteObject(password); 
                }
                else
                {
                    var pwd = new Password(PwLength).IncludeLowercase().IncludeUppercase();
                    var password = pwd.Next();
                    WriteObject(password); 
                }

                //var pwd = new Password(PwLength).IncludeLowercase().IncludeUppercase().IncludeSpecial().IncludeNumeric();
            }
            
        }

    }
}

