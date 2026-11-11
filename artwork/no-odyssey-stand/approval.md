# No-Odyssey mannequin design approval

On 2026-11-11, the user approved the south, east and north mannequin masters shown in the Codex chat. This approves their designs; layer separation, exact framing, tint review, export inspection, and in-game confirmation remain pending.

| View | Selected source | SHA-256 |
| --- | --- | --- |
| South | `sources/south-master.png` | `318FB3103BA43039AA4CA24553CDC0565755AA482AF029CBDC15E57387FEB90E` |
| East | `sources/east-master.png` | `85B047D333C5D69077CD9285EA6BDB95EEA98736BD50E2E69EAE4880980EF955` |
| North | `sources/north-master.png` | `3678D363331E9098D11461202A393FFD05B27D880BFA966F11711D26A77D7E77` |

All three were created with the built-in image generator with transparent backgrounds. The user supplied images of Odyssey's south, east and north stands as design references, overriding the earlier prompt's direction not to reference that art. SHA-256 hashes of the supplied references, respectively: `ED72A5693B9905FF658E8924551CFB6C74BDAF231A49B9A3270D82B07DA28688`, `F8FF8AE628E1701D3850F46F150ECCEC9E76685811732A5D58A5D09422C58A38`, `4E193F25CDD1842AB8ED0F6EF6EB2E7DB4F57A4BF86AD064AB09BE35BD7AA7F5`. These references were supplied in chat and are not included here. The masters are flattened.

## South generation prompt

> Repaint the provided reference as a clean neutral-greyscale RimWorld outfit stand sprite master. Match the reference silhouette and proportions closely: small squat compact object; head is a broad round oval, only slightly wider than the body and separated from it by a thick dark curved neckline; below it a straight, narrow, gently tapered tube torso, the sides slightly rounded, with a thin vertical centre seam and one horizontal seam across the middle. The body stands directly in a broad, low, flat-topped rounded rectangular plinth, no visible pole. The head has a subtle vertical centre seam and horizontal seam in the lower half. Preserve the reference's simple soft airbrushed shading, thin dark outlines, subdued texture and top-down game perspective, not glossy smooth vector art. Use light neutral greys rather than brown so the game can tint the sprite with its construction material. Full complete object centered, genuine transparent background, no clothing, no accessories, no text, no cast shadow. This is the south view master.

## East generation prompt

Inputs: approved south master as image 1; user-supplied east reference as image 2.

> Derive the EAST-facing view of the same physical mannequin in image 1, using image 2 only as a reference for east-facing orientation and silhouette. Image 1 governs its design, parts, grayscale palette, outline, seams, camera, shading, and proportions. Turn it to its side facing right: head and body side profiles lean right, matching direction reference. Preserve one round featureless head, one tapered torso and one low base, corresponding seams, and the same simple dark outline. Near-white neutral grayscale suitable for stuff tint; no brown. Complete centered object, transparent background, no added objects or text. Single sprite, not a reference sheet.

## North generation prompt

Inputs: approved south master as image 1; user-supplied north reference as image 2.

> Derive the NORTH-facing back view of the same physical mannequin in image 1, using image 2 only as a reference for north-facing orientation and silhouette. Image 1 governs its design, parts, grayscale palette, outline, seams, camera, shading, and proportions. Back view retains one round featureless head, one tapered torso and one low base, corresponding centered vertical and horizontal seams, and the same simple dark outline. Near-white neutral grayscale suitable for stuff tint; no brown. Complete centered object, transparent background, no added objects or text. Single sprite, not a reference sheet.

On 2026-11-11, the user explicitly chose to keep the Odyssey-derived seams on the approved mannequin designs.
