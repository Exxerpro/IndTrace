#!/usr/bin/env bash
# Converts the recordings and screenshots in $MEDIA_WORK into the website's assets:
#   site/assets/video/<name>.mp4 (H.264, 1280 wide, no audio) and <name>-poster.webp
#   site/assets/screens/<name>-1600.webp and <name>-800.webp
# Each video is trimmed at the start (the page settling) and gets its poster from a chosen second.
# Needs ffmpeg with libx264 and libwebp.
set -euo pipefail
source "$(dirname "$0")/../env.sh"
video_out="$REPO_ROOT/site/assets/video"
screens_out="$REPO_ROOT/site/assets/screens"
mkdir -p "$video_out" "$screens_out"

# encode <recording> <trim-start-seconds> <poster-second>
encode() {
    local input
    input=$(ls "$MEDIA_WORK/video/$1"/*.webm 2>/dev/null | head -1) || true
    if [[ -z "$input" ]]; then echo "skip $1: no recording"; return; fi
    ffmpeg -v error -y -ss "$2" -i "$input" -vf "scale=1280:-2,fps=25" -c:v libx264 -preset slow -crf 27 \
        -pix_fmt yuv420p -movflags +faststart -an "$video_out/$1.mp4"
    ffmpeg -v error -y -ss "$3" -i "$input" -frames:v 1 -vf scale=1280:-2 -c:v libwebp -quality 78 \
        "$video_out/$1-poster.webp"
    echo "$1.mp4 $(ffprobe -v error -show_entries format=duration -of csv=p=0 "$video_out/$1.mp4")s"
}
encode part-journey 1.2 44
encode define-routing 3.0 44
encode trace-part 3.0 14

for png in "$MEDIA_WORK"/screens/*.png; do
    [[ -e "$png" ]] || continue
    name=$(basename "$png" .png)
    for width in 1600 800; do
        ffmpeg -v error -y -i "$png" -vf "scale=$width:-2" -c:v libwebp -quality 80 "$screens_out/$name-$width.webp"
    done
    echo "$name screenshots"
done
