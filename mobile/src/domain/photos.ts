import * as ImagePicker from 'expo-image-picker';

/** Opens the camera or photo library. Returns the photo URI, or null if cancelled or not allowed. */
export async function pickPhoto(source: 'camera' | 'library'): Promise<string | null> {
  const perm = source === 'camera'
    ? await ImagePicker.requestCameraPermissionsAsync()
    : await ImagePicker.requestMediaLibraryPermissionsAsync();
  if (!perm.granted) return null;
  const opts: ImagePicker.ImagePickerOptions = { mediaTypes: ['images'], quality: 0.8 };
  const result = source === 'camera'
    ? await ImagePicker.launchCameraAsync(opts)
    : await ImagePicker.launchImageLibraryAsync(opts);
  return result.canceled ? null : result.assets[0]?.uri ?? null;
}
