//Common Application Javascript Validation Routines.
/********************************************************************************/
//Checks Max Length of a textarea and issues warning when exceeded. Called on ONKEYPRESS event.
function checkMaxLength(roElement, riMaxLength)
{
	var iValueLength = roElement.value.length;
	if ( iValueLength > riMaxLength - 1 ) {
		/* optional annoying error message */
		if (confirm( "You have reached the maximum character count of "+ riMaxLength +" for this field.  Click OK to truncate." ))
		{
			/* Truncate the text */
			var sTruncatedText = roElement.value.substr(0, riMaxLength);
   			roElement.value = sTruncatedText;
   			return false;
   		}
   		else
   		{
   			return true;
   		}
	}
}
/********************************************************************************/

//function popUpToolbar(querystring)
//{
//    if (window.getSelection) window.getSelection().removeAllRanges();
//    else if (document.selection) document.selection.empty();
//    winContent = window.open('TBAJAX.aspx' + querystring, 'mywindow', 'left='+((screen.width - 420) / 2)+',top='+(screen.height - 5) / 2+',status=no,titlebar=no;toolbar=no,menubar=no,directories=no,location=no,scrollbars=yes,resizable=no,width=420,height=5px')
//    winContent.focus()
//}