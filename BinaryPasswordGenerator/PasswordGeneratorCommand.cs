using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;
using PasswordGenerator;

namespace PasswordGenerator
{
    [Cmdlet(VerbsCommon.New, "Password")]
    public class GetGeneratedPasswordCommand : PSCmdlet
    {
        private int _pwLengthDefault = 16;
        [Parameter]
        [ValidateRange(4, 128)]
        public int Length
        {
            get { return _pwLengthDefault; }
            set { _pwLengthDefault = value; }
        }

        private int _amountDefault = 1;
        [Parameter]
        public int Amount
        {
            get { return _amountDefault; }
            set { _amountDefault = value; }
        }

        [Parameter]
        public SwitchParameter IncludeSpecial { get; set; }
        [Parameter]
        public SwitchParameter IncludeNumeric { get; set; }
        [Parameter]
        public SwitchParameter IncludeUppercase { get; set; }
        [Parameter]
        public SwitchParameter IncludeLowercase { get; set; }

        /* Since we're in a class already, we can define a method
        This class builds on top of the `Password` object and calls
        each method based on the bool (switch) from user input.
        */
        private Password ConfigurePasswordGenerator()
        {
            var password = new Password(Length);
            // I could use `single statement body` syntax here but decided against it
            if (IncludeSpecial)
            {
                password.IncludeSpecial();
            }

            if (IncludeNumeric)
            {
                password.IncludeNumeric();
            }

            if (IncludeUppercase)
            {
                password.IncludeUppercase();
            }

            if (IncludeLowercase)
            {
                password.IncludeLowercase();
            }

            return password;
        }

        protected override void ProcessRecord()
        {
            for (int i = 0; i < Amount; i++)
            {
                var password = ConfigurePasswordGenerator().Next();
                WriteObject(password);
            }
        }
    }
}

