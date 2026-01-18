# JapaneseVerbConjugation

## Project status
- Avalonia UI is the primary UI going forward.
- WinForms UI is deprecated and kept only as a legacy reference during transition.

## Aims
The aim of this app is to make it easier for people to study verb conjugations in Japanese. There are a lot of conjugations so this app is to help people remember how to conjugate verbs quickly. There are also many different verbs that sound similar or don't obviously fall into godan or ichidan. This should help practice that quickly and also help users learn which groups the verbs fall into (obvious when you have time to think but hard when you are less familiar or trying to talk quick)

## Current features

### Import
A user can import from a preset list of N5/N4 verbs (this will be extended to include N3-N1 as well longer term) and a custom list that a user can define if they don't want everything and wanted a more targetted experience for now.
Currently the import drags the reading from the JMDict, but this is currently imperfect and some things won't be brought in. There is no solution around this right now but the plan is to return a list of verbs which were not added and allow the user to set the group on them and also the furigana reading. This is all that is needed to allow the conjugation engine to then give you the correct answers to your attempted conjugations.

### Conjugation checks
You can type in conjugation checks into the textboxes next to the labels. This is by default set to accept Kanji only (given you have the kanji right in front of you) but you can change it to also allow hiragana if you really want. Within the options you can also cusomtise which conjugations are shown for the verbs. If you are very confident with certain conjugations or if you haven't studied certain conjugations yet you can disable them to only see what you know or want. 
We have a hint button that initially shows a user the ending of the conjugation to hopefully guide them in the correct direction. After that we have a show answer to help the user if they are really stuck.
Once a user has correctly got the conjugation we will disable the textbox and buttons for hint and check. We then have a close which shows the user the are out by one kana, either incorrect hiragana kana or just missing a kana, and then a wrong for a user being completely off.  

Longer term the plan is to track the attempts at a conjugations and try to learn what the user is good or bad at and allow them to turn on a focus mode where the app will select a few conjugations the user is bad at and then allow them to just go through these and adapt as a user get's better or worse at conjugations. Using the hint/show answer will be tracked as maybe a 2 and 5 in failed attempts to prevent missing out verbs where a user tries it once due to using the helpers.


## General info

We don't currently track too much. Longer term I want to add a way for a user to favourite a verb for it to show up after the select the fav mode so they can train a more curated list of their choosing rather than what the app may calculate they are worse at (if a user is working towards N3 they may want to focus their own selection of verbs as opposed to N2/1 they may have been bad at or N5/4 which they may know easily for example).  
I will also add an area that displays the level of the verb if it was imported from one of our prebuild files or longer term update the import so a user can add the level next to the verb in the CSV so we can still add the expected JLPT level and then also add a way to filter for verbs of a certain level or below a certain level at the users choosing.  
  
There is a previous verb and next verb button for the user to cycle through the verbs, currently the start and end of the deck simply disable the prev or next but that will be updated too loop back around.  
There is no way to shuffle the verbs at the minute but that will be solved long term. A shuffle button will be added so a user doesn't get the verbs in the same order all the time. That would possibly become more of a memory excerice than I want (although this is memory based it's about remembering the correct conjugation rules and styles, not just remembering the order of the verbs for the order of how to conjugate, I hope that sentence  makes sense to you it does to me)  

I would also like to allow the user to search through the verbs so they can pick ones they are bad at easier than filtering down to a fav list which may be rather large, and while I think about it it may be good to allow a user to filter based on the verb group as ichidan becomes very easy over time but godan can conjugate different based on the ending which can mean more practice is useful. Maybe being able to break down only showing godan and then further only showing certain endings would be nice



#### Curate this readme a little better and pull all desired features to add down here
We will also add a more powerful clear that will let the user clear all conjugations or conjugations for certain levels.  


##### I would like to note. Particularly the avalonia UI is largely coded with and heavily leaned on AI. I want to keep developing while studing Japanese and exploring Japan but, as you can imagine, taking the time to really learn Avalonia is not something I have at the minute, particularly while I work on a separate website which is where I focus more of my dev time when I have it. This app is to really just help me study Japanese so having the UI largely coded by AI and reviewed by me while I also test the actual core functionality and code a bit more of that myself just works for saving time on this. If this is upgraded into a website or app that could live on an appstore or playstore long term I will spend longer on this manually developing a lot more of the UI. For now AI is getting what I need in there in an efficient manner with minimal prompting. It's perfect for now even if it leads to some slightly more ineffiecient frontend code or slightly odd coding decisions when it comes to the UI
