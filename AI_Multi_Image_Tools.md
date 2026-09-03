# AI Tools That Process Multiple Images

## Overview

AI tools that can work with multiple images for character design, reference sheet creation, and concept art generation.

---

## Multi-Image AI Tools

### 1. Stable Diffusion (Img2Img + ControlNet)

**Best for:** Converting concept art into reference sheets, style transfer

**Multi-Image Capabilities:**
- **Img2Img:** Transform existing images while preserving composition
- **ControlNet:** Use multiple reference images to guide generation
- **IP-Adapter:** Use multiple reference images for style/character consistency
- **Reference-Only:** Generate new images based on multiple references

**How to Use:**
1. Install Stable Diffusion WebUI with ControlNet extension
2. Upload your concept art images (the 4 PNG files in your folder)
3. Use ControlNet to guide generation based on references
4. Generate reference sheet with consistent character design

**Pros:**
- Free and open source
- Full control over generation
- Can use your existing concept art
- Best for character consistency

**Cons:**
- Requires GPU
- Technical setup needed
- Learning curve

**Recommended for:** Creating reference sheet from your existing concept art

---

### 2. Leonardo.ai (Reference Image Upload)

**Best for:** Using concept art to generate variations

**Multi-Image Capabilities:**
- Upload up to 4 reference images
- AI learns from your references
- Generate consistent character variations
- Style transfer from references

**How to Use:**
1. Go to https://leonardo.ai/
2. Create new image generation
3. Upload your concept art images (up to 4)
4. Set reference strength (0.1-1.0)
5. Generate character reference sheet

**Pros:**
- Easy to use
- Good character consistency
- Free tier available
- No technical setup

**Cons:**
- Daily limit on free tier
- Less control than Stable Diffusion
- Watermark on free images

**Recommended for:** Quick character variations from your concept art

---

### 3. Midjourney (Image Prompts)

**Best for:** Style transfer, character variations

**Multi-Image Capabilities:**
- Use image URLs as prompts
- Blend multiple images together
- Image weighting for influence control
- Style transfer from references

**How to Use:**
1. Upload your concept art to Discord
2. Copy image URLs
3. Use in prompt: `[image URL 1] [image URL 2] werewolf character reference sheet`
4. Adjust with `--iw` (image weight) parameter

**Example Prompt:**
```
[URL1] [URL2] [URL3] werewolf character reference sheet, A-pose, three views, white background --iw 2
```

**Pros:**
- High quality results
- Good character consistency
- Easy to use
- Multiple image blending

**Cons:**
- Paid subscription required
- Discord-based interface
- Less control than Stable Diffusion

**Recommended for:** High-quality character variations

---

### 4. Kandinsky (Multi-Image Conditioning)

**Best for:** Free multi-image generation

**Multi-Image Capabilities:**
- Multiple image conditioning
- Style transfer
- Character consistency
- Free to use

**Website:** https://kandinsky.ai/

**How to Use:**
1. Go to Kandinsky website
2. Upload reference images
3. Set conditioning strength
4. Generate character reference sheet

**Pros:**
- Completely free
- Multiple image support
- Good quality
- Easy to use

**Cons:**
- Less popular (smaller community)
- Fewer features than competitors
- Limited control

**Recommended for:** Free multi-image generation

---

### 5. Playground AI (Image Upload)

**Best for:** Quick multi-image generation

**Multi-Image Capabilities:**
- Upload reference images
- Style transfer
- Character consistency
- 500 free images/day

**Website:** https://playgroundai.com/

**How to Use:**
1. Create account
2. Start new generation
3. Upload reference images
4. Set reference strength
5. Generate character reference sheet

**Pros:**
- Generous free tier
- Easy to use
- Good character consistency
- Multiple image support

**Cons:**
- Watermark on free images
- Less control than Stable Diffusion
- Account required

**Recommended for:** Quick multi-image generation

---

## Specialized Character Tools

### 6. Artbreeder (Character Mixing)

**Best for:** Creating character variations by mixing images

**Multi-Image Capabilities:**
- Mix multiple character images
- Adjust genetics (sliders)
- Create character families
- Consistent character design

**Website:** https://www.artbreeder.com/

**How to Use:**
1. Upload your concept art
2. Create "children" by mixing images
3. Adjust sliders for traits
4. Generate character variations

**Pros:**
- Specialized for characters
- Easy mixing interface
- Free tier available
- Good for character families

**Cons:**
- Limited to character faces
- Not for full-body reference
- Account required
- Limited free tier

**Recommended for:** Character face variations, not full reference sheets

---

### 7. CharGen (Character Generator)

**Best for:** RPG character generation from references

**Multi-Image Capabilities:**
- Upload reference images
- Generate character portraits
- Style transfer
- Character consistency

**Website:** Various (search "CharGen AI")

**Pros:**
- Specialized for characters
- Good for RPG portraits
- Reference-based generation

**Cons:**
- May be paid
- Limited to portraits
- Variable quality

**Recommended for:** Character portraits, not full reference sheets

---

## Workflow for Your Concept Art

### Using Your 4 Concept Art Images

**Option 1: Stable Diffusion with ControlNet (Best Quality)**

1. **Install Stable Diffusion WebUI**
   - Download from GitHub
   - Install ControlNet extension
   - Install IP-Adapter extension

2. **Prepare Your Images**
   - Your 4 concept art PNG files:
     - `11a8fd5c-45dc-40bf-98bf-e7399aa76646.png`
     - `1626df0a-6588-4eef-a3e8-6a78dbdbcc8a.png`
     - `2d98495d-d0e6-4bab-9504-0f0d544a656b.png`
     - `d6f38d8d-f963-46dd-b6f9-bab743e9c2a5.png`

3. **Use ControlNet**
   - Upload all 4 images as references
   - Use "Reference-Only" or "IP-Adapter"
   - Set reference strength to 0.6-0.8
   - Generate reference sheet with A-pose

4. **Generate Reference Sheet**
   - Use prompt from Image_Generation_Prompts.md
   - Set dimensions: 1536x512
   - Generate multiple variations
   - Select best result

**Option 2: Leonardo.ai (Easiest)**

1. **Upload References**
   - Go to https://leonardo.ai/
   - Create new image generation
   - Upload all 4 concept art images
   - Set reference strength to 0.7

2. **Generate**
   - Use prompt from Image_Generation_Prompts.md
   - Set aspect ratio to 3:1
   - Generate 4 variations
   - Download best result

**Option 3: Midjourney (Best Quality, Paid)**

1. **Upload to Discord**
   - Upload your 4 concept art images
   - Copy image URLs

2. **Generate**
   - Use prompt with image URLs
   - Set image weight with `--iw 2`
   - Generate reference sheet
   - Download best result

---

## Step-by-Step Guide for Your Project

### Step 1: Prepare Your Concept Art
- Your 4 concept art images are ready
- They show the werewolf character from different angles
- Good reference material for AI generation

### Step 2: Choose Tool
- **Best Free:** Leonardo.ai or Kandinsky
- **Best Quality:** Stable Diffusion (local) or Midjourney (paid)
- **Easiest:** Leonardo.ai

### Step 3: Upload References
- Upload all 4 concept art images
- Set reference strength (0.6-0.8 recommended)
- Arrange for character consistency

### Step 4: Generate Reference Sheet
- Use prompt from Image_Generation_Prompts.md
- Set aspect ratio to 3:1 (for three views)
- Generate 4-8 variations
- Review results

### Step 5: Select and Refine
- Choose best variation
- Regenerate with adjustments if needed
- Use best result as final reference sheet

---

## Tips for Best Results

### Using Multiple References
- Use all 4 concept art images for consistency
- Set reference strength to 0.6-0.8 (not too high, not too low)
- Generate multiple variations
- Select best result

### Character Consistency
- Use same seed for all generations
- Keep reference images consistent
- Use same model and settings
- Generate in batches

### Reference Sheet Specifics
- Specify "A-pose" in prompt
- Specify "three views: front, back, side"
- Specify "white background"
- Specify "character reference sheet"

---

## Comparison Table

| Tool | Multi-Image | Free | Quality | Difficulty | Best For |
|------|-------------|------|---------|------------|----------|
| Stable Diffusion | Yes | Yes | High | Hard | Professional results |
| Leonardo.ai | Yes | Yes | Medium | Easy | Quick results |
| Midjourney | Yes | No | High | Medium | High quality |
| Kandinsky | Yes | Yes | Medium | Easy | Free generation |
| Playground AI | Yes | Yes | Medium | Easy | Quick variations |
| Artbreeder | Yes | Yes | Medium | Easy | Face variations |

---

## Recommended Workflow for You

**If you have a GPU:**
1. Install Stable Diffusion WebUI
2. Install ControlNet and IP-Adapter
3. Upload your 4 concept art images
4. Generate reference sheet with full control

**If you want easy and free:**
1. Go to Leonardo.ai
2. Upload your 4 concept art images
3. Generate reference sheet
4. Download best result

**If you want best quality (paid):**
1. Upload concept art to Discord
2. Use Midjourney with image prompts
3. Generate reference sheet
4. Download best result

---

## Post-Processing

After generating your reference sheet:

1. **Arrange Views** (if generated separately)
   - Use GIMP or Photoshop
   - Arrange front, back, side views horizontally
   - Add white background if needed

2. **Add Annotations**
   - Add measurements
   - Label armor pieces
   - Mark rune locations
   - Add fur direction indicators

3. **Refine Details**
   - Paint over any issues
   - Add missing details
   - Adjust colors if needed
   - Clean up artifacts

---

*Document Version: 1.0*  
*Last Updated: May 2026*
