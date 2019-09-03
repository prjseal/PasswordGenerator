
 ___  __    __   __  _   _  __  ___ __     __ ___ __  _ ___ ___  __ _____ __  ___  
| _,\/  \ /' _//' _/| | | |/__\| _ \ _\   / _] __|  \| | __| _ \/  \_   _/__\| _ \ 
| v_/ /\ |`._`.`._`.| 'V' | \/ | v / v | | [/\ _|| | ' | _|| v / /\ || || \/ | v / 
|_| |_||_||___/|___/!_/ \_!\__/|_|_\__/   \__/___|_|\__|___|_|_\_||_||_| \__/|_|_\ 
                                                                            																			  
-----------------------------------------------------------------------------------------

A library which generates random passwords with different settings to meet the OWASP requirements

Usage Example

// If you don't pass it any settings it will return a 16 character password

var pwd = new Password();
var result = pwd.Next();

// Here is how to get lowercase, uppercase and special characters using the fluent approach
// This example also passes the length in using the fluent method LengthRequired()

var pwd = new Password().IncludeLowercase().IncludeUppercase().IncludeSpecial().LengthRequired(128);
var result = pwd.Next();

-----------------------------------------------------------------------------------------

   ( (
    ) )
  ........    If you want show your appreciation for this package, you can buy me a coffee
  |      |]   
  \      /    https://codeshare.co.uk/coffee
   `----'