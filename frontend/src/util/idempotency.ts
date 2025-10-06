export function generateIdempotencyKey(dto: object): Promise<string> {
  const dataString = JSON.stringify(dto);

  const encoder = new TextEncoder();
  const data = encoder.encode(dataString);
  
  return crypto.subtle.digest('SHA-256', data)
    .then(hashBuffer => {
      const hashArray = Array.from(new Uint8Array(hashBuffer));
      const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
      
      return `key_${hashHex.substring(0, 50)}`;
    });
}