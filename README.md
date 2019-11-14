# RC210_Data_Assistant
This project supports the Arcom Controllers model RC210 repeater controller by providing the user 
with english decoded data from the numerical output of the download from the controller. The output
is in two forms. First is an html file and second is the printed output of that file.

F/W up through 7.64 is currently supported by the program and the .xml files. You only need the XML file(s) that support the f/w versions you support. And beginning with the release of version 7.00 f/w the XML files need to be in the XML folder in the same directory as the program file. The versions supported are:
  Ver5_6_macro.def.xml - 5.281 - 6.045
  Ver7_macro_def.xml - 7.00 - 7.361
  Ver8_macro_def.xml - 7.39 - 7.612
  Ver9_macro_def.xml - 7.63 - 7.64

Current version of the program, ver 2.0.1.5, now currectly displays the Voice IDs as InitialIDs and PendingIDs. this change was actually made with f/w 7.36. Be sure to check the HISTORY.TXT file for the f/w version you have deployed.

James Bolding, KC5TDG, is the originator of this project and Bob Norris, AK5U, has been doing 
updates as changes in firmware have been released over the past years.
