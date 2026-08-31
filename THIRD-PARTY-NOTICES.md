# Third Party Notices

Bardic Memory is licensed under the GNU Affero General Public License v3, see LICENSE.md.
It includes work derived from the projects below, under their own terms.

## Orchestrion Plugin

https://github.com/perchbirdd/OrchestrionPlugin

Bardic Memory drives the games background music scene list and how to do that was worked
out by the Orchestrion plugin long before this one existed. The following are derived from
it rather than discovered independently:

* `BardicMemory/Interop/BgmInterop.cs` - the order the scene fields are written in to start
  a track and the stop path that zeroes the ids and sets the Resume flag so the zone music
  comes back. Both from `Orchestrion/BGMSystem/BGMController.cs` (`SetSong`). The scene flag
  bit values are from `Orchestrion/BGMSystem/SceneFlags.cs` and the field offsets were
  confirmed against `Orchestrion/BGMSystem/BGMScene.cs`.
* `BardicMemory/Services/PlaybackService.cs` - re-applying the driven scene every frame when
  the game reclaims it, from `BGMController.Update`. Also refusing to play at all while an
  estate orchestrion is sounding, from `Orchestrion/Audio/BGMManager.cs`.
* `BardicMemory/Interop/OrchestrionInterop.cs` - reading the estate orchestrion state,
  modelled on `Orchestrion/InnSystem/OrchestrionInnController.cs`.
* `BardicMemory/Data/OrchestrionBgmMap.csv` - generated offline by matching orchestrion roll
  names against the song titles in Orchestrion's bundled `xiv_bgm_en.csv`. Only the resulting
  pairs of row ids are shipped. No third party text is redistributed, because that song data
  comes from a community spreadsheet with no stated licence.

Licence text as published by the project:

```
MIT License

Copyright (c) 2020 Meli

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense and/or sell
copies of the Software and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
