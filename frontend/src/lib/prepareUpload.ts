const MAX_EDGE = 2400;
const RESIZE_ABOVE_BYTES = 3 * 1024 * 1024;

/** Shrinks large photos before upload so phone pictures fit the reader's limits; other files pass through. */
export async function prepareUpload(file: File): Promise<File> {
  if (!file.type.startsWith("image/") || file.size <= RESIZE_ABOVE_BYTES) return file;
  try {
    const bitmap = await createImageBitmap(file);
    const scale = Math.min(1, MAX_EDGE / Math.max(bitmap.width, bitmap.height));
    const canvas = document.createElement("canvas");
    canvas.width = Math.round(bitmap.width * scale);
    canvas.height = Math.round(bitmap.height * scale);
    canvas.getContext("2d")?.drawImage(bitmap, 0, 0, canvas.width, canvas.height);
    const blob = await new Promise<Blob | null>((resolve) => canvas.toBlob(resolve, "image/jpeg", 0.88));
    return blob ? new File([blob], file.name.replace(/\.\w+$/, ".jpg"), { type: "image/jpeg" }) : file;
  } catch {
    return file;
  }
}
