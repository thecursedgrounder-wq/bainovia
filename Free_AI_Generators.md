# Free AI Image Generators for Character Reference Sheet

## Free AI Image Generators

### 1. Stable Diffusion (Free & Open Source)

**Best for:** Character reference sheets, detailed control

**How to use:**
- **Automatic1111 WebUI:** Download and run locally (free, requires GPU)
- **ComfyUI:** Node-based interface (free, requires GPU)
- **Google Colab:** Free cloud access (limited GPU time)
- **Hugging Face:** Free online demo (limited generations)

**Pros:**
- Completely free and open source
- Full control over parameters
- Can run locally (no limits)
- Supports custom models/LoRAs
- Best for character design

**Cons:**
- Requires powerful GPU for local use
- Technical setup required
- Cloud versions have limits

**Recommended for:** Character reference sheets (use prompts from Image_Generation_Prompts.md)

---

### 2. Leonardo.ai (Free Tier)

**Best for:** Character design, game assets

**Free Tier:**
- 150 credits/day (approximately 15-30 images)
- Good quality character generation
- User-friendly interface

**Website:** https://leonardo.ai/

**Pros:**
- Easy to use
- Good character generation
- Daily free credits
- No technical setup

**Cons:**
- Daily limit on free tier
- Watermark on free images
- Less control than Stable Diffusion

**Recommended for:** Quick character concepts, testing prompts

---

### 3. Bing Image Creator (DALL-E 3)

**Best for:** Quick character concepts

**Free Tier:**
- 15 boosts/day (faster generation)
- Unlimited free generations (slower after boosts)
- Uses DALL-E 3 model

**Website:** https://www.bing.com/images/create

**Pros:**
- Completely free
- Uses DALL-E 3 (high quality)
- Easy to use (no account needed)
- Good for quick concepts

**Cons:**
- Limited control over parameters
- No aspect ratio control
- Slower after daily boosts
- Can't specify negative prompts

**Recommended for:** Quick character concepts, initial ideas

---

### 4. Playground AI

**Best for:** Character design, style exploration

**Free Tier:**
- 500 images/day
- Good quality generation
- Multiple style options

**Website:** https://playgroundai.com/

**Pros:**
- Generous free tier
- Good character generation
- Style presets
- Easy to use

**Cons:**
- Watermark on free images
- Limited control
- Account required

**Recommended for:** Style exploration, character variations

---

### 5. Civitai

**Best for:** Finding character models, community resources

**Free Tier:**
- Access to community models
- Free image generation with some models
- Model sharing and downloading

**Website:** https://civitai.com/

**Pros:**
- Huge library of character models
- Community-driven
- Can download models for local use
- Free generation with some models

**Cons:**
- Quality varies by model
- Requires account
- Some models require credits

**Recommended for:** Finding character-specific models, LoRAs

---

### 6. Hugging Face Spaces

**Best for:** Testing different models

**Free Tier:**
- Access to various Stable Diffusion models
- Free generation (limited by queue)
- No account required for some spaces

**Website:** https://huggingface.co/spaces

**Pros:**
- Variety of models available
- Free to use
- No setup required
- Can test different styles

**Cons:**
- Queue times can be long
- Limited generations per session
- Less control than local setup

**Recommended for:** Testing different models, quick generations

---

## Recommended Workflow for Character Reference Sheet

### Option 1: Best Quality (Stable Diffusion Local)

1. **Install Automatic1111 WebUI**
   - Download from GitHub
   - Requires NVIDIA GPU (8GB+ VRAM recommended)
   - Follow installation guide

2. **Use Character-Specific Model**
   - Download character model from Civitai
   - Recommended: "Realistic Vision" or "DreamShaper"
   - Install in WebUI

3. **Generate Reference Sheet**
   - Use prompts from Image_Generation_Prompts.md
   - Set dimensions: 1536x512 (for three views)
   - Use DPM++ 2M Karras sampler
   - CFG Scale: 7
   - Steps: 40

### Option 2: Easy & Free (Bing Image Creator)

1. **Go to Bing Image Creator**
   - https://www.bing.com/images/create
   - Sign in with Microsoft account

2. **Use Simplified Prompt**
   - Copy main prompt from Image_Generation_Prompts.md
   - Simplify for DALL-E 3 limitations
   - Focus on character description

3. **Generate**
   - Generate multiple variations
   - Best for single views
   - Combine in post-processing

### Option 3: Balanced (Leonardo.ai)

1. **Create Account**
   - https://leonardo.ai/
   - Get 150 daily credits

2. **Use Character Model**
   - Select "Leonardo Phoenix" model
   - Or use custom character model

3. **Generate**
   - Use prompts from Image_Generation_Prompts.md
   - Set aspect ratio to 3:1 for reference sheet
   - Generate multiple variations

---

## Prompt Adaptation for Free Tools

### For Bing Image Creator (DALL-E 3)

**Original:**
```
Character reference sheet, werewolf werebear hybrid named Kaelar, dark charcoal gray fur with silver streaks, muscular hunched build, 8.5 feet tall, tribal armor with glowing electric blue rune carvings, massive claws with glowing tips, glowing electric blue eyes, noble expressive face, A-pose standing pose, three views arranged horizontally: front view, back view, side view, clean white background, professional character design sheet, high detail, 8K, stylized realism
```

**Simplified for DALL-E 3:**
```
A professional character reference sheet showing a werewolf werebear hybrid in A-pose from three angles: front, back, and side view. The character has dark charcoal gray fur with silver streaks, muscular hunched build, tribal armor with glowing electric blue rune carvings, massive claws with glowing tips, and glowing electric blue eyes with a noble face. Clean white background. High detail concept art.
```

### For Leonardo.ai

**Use original prompts** from Image_Generation_Prompts.md with these settings:
- Model: Leonardo Phoenix
- Aspect Ratio: 3:1 (for reference sheet)
- Guidance Scale: 7
- Number of Images: 4

### For Stable Diffusion (Free Online)

**Use prompts from Image_Generation_Prompts.md** with these settings:
- Model: Realistic Vision v5.1
- Steps: 30
- CFG Scale: 7
- Sampler: DPM++ 2M Karras
- Size: 1024x1024 (single view) or 1536x512 (reference sheet)

---

## Free Character-Specific Models

### Civitai Models (Download for Stable Diffusion)

**For Realistic Characters:**
- Realistic Vision v5.1
- DreamShaper v8
- CyberRealistic

**For Stylized Characters:**
- Anything v5
- GhostMix
- RevAnimated

**For Game Characters:**
- RPG v4
- Character Design
- Game Asset Generator

**How to Use:**
1. Download model from Civitai
2. Install in Stable Diffusion WebUI
3. Select model in settings
4. Generate with character prompts

---

## Quick Start Guide

### Fastest Option (5 minutes)
1. Go to https://www.bing.com/images/create
2. Use simplified prompt
3. Generate 4 variations
4. Download best result

### Best Free Option (15 minutes)
1. Go to https://leonardo.ai/
2. Create account
3. Use main reference sheet prompt
4. Set aspect ratio 3:1
5. Generate 4 variations
6. Download best result

### Professional Option (1 hour setup)
1. Install Stable Diffusion WebUI
2. Download character model from Civitai
3. Use full prompts from Image_Generation_Prompts.md
4. Generate with full control
5. Iterate and refine

---

## Tips for Best Results

### General Tips
- Use clear, descriptive prompts
- Specify "white background" explicitly
- Mention "character reference sheet" or "turnaround"
- Include style keywords (concept art, game production)
- Generate multiple variations

### For Reference Sheets
- Generate each view separately if three-view fails
- Combine in post-processing
- Add annotations in image editor
- Use consistent style across all views

### For Character Consistency
- Use same seed for all views
- Use same model and settings
- Keep prompt consistent
- Generate in batches

---

## Post-Processing Tools (Free)

### GIMP (Free)
- Arrange multiple views into reference sheet
- Add annotations and measurements
- Adjust colors and contrast
- Remove watermarks

### Krita (Free)
- Paint over generated images
- Add details and corrections
- Create turnarounds
- Add technical annotations

### Photopea (Free Online)
- Browser-based image editor
- Similar to Photoshop
- Good for quick edits

---

## Summary

| Tool | Free Tier | Best For | Difficulty |
|------|-----------|----------|------------|
| Stable Diffusion (Local) | Unlimited | Character reference sheets | Hard |
| Leonardo.ai | 150 credits/day | Character design | Easy |
| Bing Image Creator | 15 boosts/day | Quick concepts | Very Easy |
| Playground AI | 500 images/day | Style exploration | Easy |
| Civitai | Varies | Finding models | Medium |

**Recommendation:** Start with Bing Image Creator for quick concepts, then use Leonardo.ai for better quality, or set up Stable Diffusion locally for professional results.

---

*Document Version: 1.0*  
*Last Updated: May 2026*
