using System;
using System.Management.Automation;
using PasswordGenerator;

namespace PasswordGenerator
{
    [Cmdlet(VerbsCommon.New,"Password")]
    public class GetGeneratedPasswordCommand : PSCmdlet
    {
        private int _pwLengthDefault = 16;
        [Parameter]
        [ValidateRange(4,128)]
        public Int32 Length
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
        public SwitchParameter IncludeSpecial { get; set; }
        [Parameter]
        public SwitchParameter IncludeNumeric { get; set; }
        [Parameter]
        public SwitchParameter IncludeUppercase { get; set; }
        [Parameter]
        public SwitchParameter IncludeLowercase { get; set; }

        protected override void ProcessRecord()
        {
            for (int i = 0; i < Amount; i++)
            {
                if (!IncludeLowercase & !IncludeUppercase & IncludeSpecial & IncludeNumeric)
                {
                    var pwd = new Password(Length).IncludeSpecial().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password); 
                }
                else if (IncludeNumeric & !IncludeSpecial & !IncludeUppercase & !IncludeLowercase)
                {
                    var pwd = new Password(Length).IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password);
                }
                else if (IncludeSpecial & !IncludeNumeric & !IncludeUppercase & !IncludeLowercase)
                {
                    var pwd = new Password(Length).IncludeSpecial();
                    var password = pwd.Next();
                    WriteObject(password); 
                }
                else if (!IncludeNumeric & !IncludeSpecial & IncludeUppercase & IncludeLowercase)
                {
                    var pwd = new Password(Length).IncludeLowercase().IncludeUppercase();
                    var password = pwd.Next();
                    WriteObject(password);
                }
                else if (!IncludeNumeric & !IncludeSpecial & !IncludeLowercase & IncludeUppercase)
                {
                    var pwd = new Password(Length).IncludeUppercase();
                    var password = pwd.Next();
                    WriteObject(password);
                }
                else if (!IncludeNumeric & !IncludeSpecial & !IncludeUppercase & IncludeLowercase)
                {
                    var pwd = new Password(Length).IncludeLowercase();
                    var password = pwd.Next();
                    WriteObject(password);
                }
                else if (!IncludeNumeric & IncludeLowercase & IncludeUppercase & IncludeSpecial)
                {
                    var pwd = new Password(Length).IncludeLowercase().IncludeUppercase().IncludeSpecial();
                    var password = pwd.Next();
                    WriteObject(password);
                }
                else if (!IncludeNumeric & IncludeLowercase & IncludeUppercase & IncludeNumeric)
                {
                    var pwd = new Password(Length).IncludeLowercase().IncludeUppercase().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password);
                }
                else if (!IncludeNumeric & !IncludeUppercase & IncludeSpecial & IncludeLowercase)
                {
                    var pwd = new Password(Length).IncludeLowercase().IncludeSpecial();
                    var password = pwd.Next();
                    WriteObject(password);
                }
                else if (!IncludeSpecial & !IncludeUppercase & IncludeLowercase & IncludeNumeric)
                {
                    var pwd = new Password(Length).IncludeLowercase().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password);
                }
                else if (!IncludeLowercase & !IncludeNumeric & IncludeUppercase & IncludeSpecial)
                {
                    var pwd = new Password(Length).IncludeUppercase().IncludeSpecial();
                    var password = pwd.Next();
                    WriteObject(password);
                }
                else if (IncludeLowercase & IncludeNumeric & IncludeUppercase & IncludeSpecial)
                {
                    var pwd = new Password(Length).IncludeLowercase().IncludeUppercase().IncludeSpecial().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password); 
                }
                else if (!IncludeUppercase & IncludeLowercase & IncludeNumeric & IncludeSpecial)
                {
                    var pwd = new Password(Length).IncludeLowercase().IncludeSpecial().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password); 
                }
                else if (!IncludeLowercase & IncludeUppercase & IncludeNumeric & IncludeSpecial)
                {
                    var pwd = new Password(Length).IncludeUppercase().IncludeSpecial().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password); 
                }
                else
                {
                    var pwd = new Password(Length).IncludeLowercase().IncludeUppercase().IncludeSpecial().IncludeNumeric();
                    var password = pwd.Next();
                    WriteObject(password); 
                }
            }
            
        }

    }
}

