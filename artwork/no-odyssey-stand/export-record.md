# No-Odyssey export record

The approved generated masters and their prompts are recorded in approval.md. The accepted south/east/north masters were cropped with contour masks for head and torso. The original base is occluded by the torso, so the base below was generated separately from the south master and reused in all views. The child is a uniform reduction of the visible adult torso, plus the explicitly authorized short drawn support. The north body is shifted upward 2 pixels to close the neck; a dark backing at the neck fills the joining gap. These flattened images are not independently editable source layers.

## Additional generated sources

- base-component.png SHA-256 `C7A816D9B1142FBF11342B3C51B3CD707AFEF3ED467651F12CD1CAB807F24665`
- swap-source.png SHA-256 `048F442D8EB53ACA5B12B26B622445129CC46326591346FCB95E1D34BDE00EC3`

Both made with built-in imagegen with transparency. Base prompt: "Generate only the low rounded rectangular plinth/base from this approved mannequin, isolated on transparent background. Remove head and body completely; show a clean continuous top surface where they stood. Preserve the base's size relative to itself, silhouette, perspective, neutral grey palette, dark outline, shading and style. No text, no other objects."

Swap prompt: "RimWorld game command icon: one simple shirt with two clearly distinct opposing swap arrows, one pointing left and one pointing right. Fixed colors allowed: muted light grey shirt, subdued gold arrows, charcoal contours. Broad flat shading, legible at 64 pixels, top-down game UI sprite, isolated whole icon, transparent background, no text, no backdrop, no other objects."

## Recipe and checks

Run `python artwork/no-odyssey-stand/export.py` from the project root. It writes a staged `artwork/no-odyssey-stand/export/` tree and `review.png`; copy the staged PNGs into `NoOdyssey/Textures/`. It imports the existing stand_preview() compositor. Crops are scaled uniformly once with Lanczos, then placed in transparent canvases. A shared monotonic luminance lookup preserves dark greys and maps 134 to 216 and 180 to 230, capping highlights at 245. It thickens each contour inward by up to two pixels while preserving alpha silhouette. Icons are composites resized to 64 pixels. The tint sheet is a CPU multiply approximation, not a native shader or in-game test.

All 19 staged files are RGBA and have the dimensions shown. Bounds below are Pillow alpha boxes (right/bottom exclusive); they are approximate and differ from the brief where aspect ratio or support placement requires it. The base is shallower than the suggested 35-pixel box; child south starts at y49 and ends at y96 including support; north child includes the 2-pixel join adjustment. There is no in-game validation of clothing or hats.

| PNG relative to NoOdyssey/Textures | Size | Alpha bounds | SHA-256 |
| --- | --- | --- | --- |
| `Things/Building/OutfitStand/KidOutfitStand_MenuIcon_east.png` | 64x64 | (17, 12, 46, 61) | `EFBE683EE975740FA48788D8D5C55C6219B9377796A88143322E68A863EF5676` |
| `Things/Building/OutfitStand/KidOutfitStand_MenuIcon_north.png` | 64x64 | (18, 12, 46, 61) | `04D9CE76AB7CE9CB940359EBAAF8817E693A9A4BED6A9802E2F5C88E610FDE0C` |
| `Things/Building/OutfitStand/KidOutfitStand_MenuIcon_south.png` | 64x64 | (18, 12, 46, 61) | `DDEA18D12D3C36BDB88DD21206CE09E47682B6F1D96477F1FADCED087A2B4EDE` |
| `Things/Building/OutfitStand/OutfitStand_Base_east.png` | 128x128 | (34, 88, 92, 114) | `1FE23330B5A3076B3AAD3BAC32C14031C2B2C554C16E37B68B2EA9C3AA236D95` |
| `Things/Building/OutfitStand/OutfitStand_Base_north.png` | 128x128 | (34, 88, 92, 114) | `1FE23330B5A3076B3AAD3BAC32C14031C2B2C554C16E37B68B2EA9C3AA236D95` |
| `Things/Building/OutfitStand/OutfitStand_Base_south.png` | 128x128 | (34, 88, 92, 114) | `1FE23330B5A3076B3AAD3BAC32C14031C2B2C554C16E37B68B2EA9C3AA236D95` |
| `Things/Building/OutfitStand/OutfitStand_Body_east.png` | 128x128 | (44, 42, 84, 108) | `9E5AB0A55549FA07ECB7C027AF711E23B63FA948D2C02D75EC6832DD8CB93BFC` |
| `Things/Building/OutfitStand/OutfitStand_Body_north.png` | 128x128 | (39, 42, 89, 106) | `A37B6B98E5F3CFE60F18BF08A137F9CC6350292D9152FE6532E6CBEEBDF09365` |
| `Things/Building/OutfitStand/OutfitStand_Body_south.png` | 128x128 | (39, 42, 89, 108) | `887D84262BEED7DC9BE898E38506B6736608310FD1ED873BACC3B4DC0FECCAA3` |
| `Things/Building/OutfitStand/OutfitStand_BodyChild_east.png` | 128x128 | (48, 42, 79, 96) | `08EB0DB68FE6ED51D0C9D5D35D6EB9A2F6362B0FD108059BF723B7905E5FD8A0` |
| `Things/Building/OutfitStand/OutfitStand_BodyChild_north.png` | 128x128 | (49, 42, 79, 94) | `BA4BF275CD8102E4EC560064E6D8B8D1D82BAF37DB998CA4AE8D195FA74D5A4F` |
| `Things/Building/OutfitStand/OutfitStand_BodyChild_south.png` | 128x128 | (49, 42, 79, 96) | `830E88DA363C6817EB77A51A206BACFEB8BBD7E2A2AA49B801EC941F43333D83` |
| `Things/Building/OutfitStand/OutfitStand_Head_east.png` | 128x128 | (42, 41, 86, 88) | `508D9ED5061101FEF23BE83A72243106484D9D419DEC0A44D56C5A11B48683F4` |
| `Things/Building/OutfitStand/OutfitStand_Head_north.png` | 128x128 | (42, 40, 86, 88) | `05A8AE11B34D08FE7628DBCB27D938927A9AA357C210979739E2616AAC1CB573` |
| `Things/Building/OutfitStand/OutfitStand_Head_south.png` | 128x128 | (42, 40, 86, 89) | `D3A836E0DAB34EFB2FA6F3BFF137B204FE0EAA1AE4A3E53338E7FF8D2BC3E3C5` |
| `Things/Building/OutfitStand/OutfitStand_MenuIcon_east.png` | 64x64 | (15, 3, 49, 61) | `06CB491FC04615D188D82CC1E68FC7C44E13DC4E904ACB30E00DA82C87F51E7D` |
| `Things/Building/OutfitStand/OutfitStand_MenuIcon_north.png` | 64x64 | (15, 3, 49, 61) | `6638184D11316F671587B09F7E69F557A149F81823B24172F790DBABBF2952C1` |
| `Things/Building/OutfitStand/OutfitStand_MenuIcon_south.png` | 64x64 | (15, 3, 49, 61) | `59C487FEF14CCE3CC2CE138FAEE7CA55C49C62CC3FF745A391A4B342E32B8B92` |
| `UI/Commands/SwapOutfits.png` | 128x128 | (6, 14, 121, 114) | `DB1EB3F6C57E27FE65A789EE2D631B05FA38AEF07466DB02AF4CFB44A6AD9528` |
