# FaeleiHZMIDI
## About

<div style="text-align: justify;">

*A quick and versatile tool to make those trendy HZ Bass in your MIDIs, be it an "impossible remix" or not!*

This page uses the term "Hertzchop" which means "to chop a MIDI note or set of MIDI notes into a sequence of notes, that when played, resonates a frequency that aligns to its pitch (`NoteNumber`).

## Usage
Faelei HZ MIDI uses `textEvent` MIDI Events to determine which sections of the MIDI to be Hertzchopped. 

If you want an even quicker tool (that Hertzchops the ENTIRE MIDI and not just sections), use this [hzchop tool made by GamingMIDI from the BMC](https://cdn.discordapp.com/attachments/342003805270966284/1367762328690626640/hzchop.exe?ex=68a22b2e&is=68a0d9ae&hm=be7fdfe9bf5f91fad0c67a5823affac0ec42b86991ee49405d59187017b51121&) instead. 

See below for the Keywords used:



| Keywords | Actions                                                                                                                                                                                                                     | Usage                                                                                                                                                                       | Tips                                                                                                                                                                                                                                               |
|-------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| beginFreq               | Marks the start of the Hertzchop Section. Notes past this event (and track the event is on) will be Hertzchopped.                                                                                                           | Place this event before the set of notes you want to Hertzchop.                                                                                                             | Sections can span thru tracks, just make sure that the beginFreq event is located at the first track of the Hertzchop Section and the endFreq event is located at the last track.<br/><br/>There can be multiple Hertzchop sections, not just one! |
| endFreq                 | Marks the end of the Hertzchop Section. Notes past this event (and track the event is on) will no longer be Hertzchopped.                                                                                                   | Place this event after the set of notes you want to Hertzchop.                                                                                                              |                                                                                                                                                                                                                                                    |
| octaveSet [octave]      | Sets the deltaOctave variable to [octave]. Higher values will yield higher frequencies and lower note lengths. It is recommended to have this value set to any value under zero, especially for MIDIs with low resolutions. | Place this event before the set of notes you want to Hertzchop. Alternatively, this event can be placed on the first track if the settings will be used for the whole MIDI. | There can be multiple octaveSet events anywhere, meaning you can configure a track dedicated for bass to have a higher frequency, and the track dedicated for melody to have a lower frequency.                                                    |
| enableFreqWrap          | Enables Frequency Wrapping by a Wrapping Coefficient to ensure that the output length of Hertzchopped notes are within allowable ranges.                                                                                    | Place this event at the first track. This feature is a global feature and cannot be disabled once enabled.                                                                  | You can use this, in conjunction with octaveSet and coefficientWrap events for even more customization.                                                                                                                                            |
| coefficientWrap [value] | Sets the Wrapping Coefficient to [value]. This value has to be between 0 and 1.                                                                                                                                             | Place this event at the first track. This can be placed before a Hertzchop section.                                                                                         | There can be multiple coefficientWrap events everywhere, for similar reasons stated in the octaveSet tips section.                                                                                                                                 |


In summary, Using keywords `beginFreq` and `endFreq`, you can now quickly make MIDI notes have the rendered HZ feel.

## FAQ

<ol>
<li><b>When I run the program, it shows an error. Why does this happen?</b><br><em>You need to drag and drop the MIDI to the program itself. Then the program will work.</em></li>
<li><b>When I run the program with the MIDI, nothing changes! No MIDIs were "Hertzchopped" and I got a warning telling me so. Is there a fix?</b><br><em>You did not add the keywords in the form of text events in your MIDI. Make sure your MIDI contains the keywords before you run the program with it.</em></li>
<li><b>When I run the program with the MIDI, I receive an error that talks about a </b><span style="font-family: 'Lucida Console', Monaco, monospace;background-color: #707070;padding: 2px;">Note Off without a Note On</span><b>. What can I do?</b><br><em>This is a limitation of NAudio and not my program. While I am working on making my own MIDI File Reader to hopefully ignore this issue, you would have to load the MIDI in a MIDI Editor and re-export it back. This is the best method I can recommend in the meantime.</em></li>
<li><b>Where can I put a text event on my MIDI?</b><br><em>You have to use either MIDIEditor or Domino MIDI Editor programs. For Domino, head to </em><span style="font-family: 'Lucida Console', Monaco, monospace;background-color: #707070;padding: 2px;">Insert > Lyrics</span><em> and then add the keyword. It does not matter if the text event is a Lyrics event, it will work. Theoretically, you could set the track name to be one of the keywords and it could potentially work.</em></li>
<li><b>The MIDI is finally Hertzchopped. Is it openable in MIDI Editors?</b><br><em>Now that your MIDI has been Hertzchopped, it should be openable in MIDI Editors as only note events were manipulated.</em></li>
<li><b>Will you add the option to change the MIDI Resolution?</b><br><em>I might add an option to set the resolution of the output MIDI, which is available in GamingMIDI's hzchop program. However, considering that this tool is intended to be used while editing or making MIDIs, you could simply change the resolution of the MIDI in the MIDI Editor instead.</em></li>
</ol>

## Credits

"NAudio" is created and copyrighted by Mark Heath and contributors.

"FaeleiHZMIDI" is created by Daniferous and is under a [Creative Commons License BY-NC-SA](https://creativecommons.org/licenses/by-nc-sa/4.0/).

<img src="https://mirrors.creativecommons.org/presskit/buttons/88x31/png/by-nc-sa.png" alt="Creative Commons License BY-NC-SA" width="161" height="56">

</div>