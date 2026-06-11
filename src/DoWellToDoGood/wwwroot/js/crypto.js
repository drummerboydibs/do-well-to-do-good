// Zero-knowledge envelope encryption for Do Well to Do Good.
//
// passphrase --PBKDF2(600k)--> KEK --unwraps--> DEK --AES-GCM--> entries
// recovery code --PBKDF2(600k)--> RK --unwraps--> the same DEK
//
// The DEK lives ONLY in this closure, in memory, for the current tab.
// It is never persisted, never sent anywhere, and is wiped on lock(),
// sign-out, or page close. The server stores only salts, wrapped
// (encrypted) keys, and ciphertext.
window.dwtdgCrypto = (() => {
    let dek = null; // active data-encryption key (non-extractable CryptoKey)

    const te = new TextEncoder(), td = new TextDecoder();
    const ITERATIONS = 600000; // PBKDF2-SHA256, OWASP-recommended order of magnitude

    function toB64(buf) {
        const bytes = new Uint8Array(buf);
        let s = "";
        const CHUNK = 0x8000;
        for (let i = 0; i < bytes.length; i += CHUNK)
            s += String.fromCharCode.apply(null, bytes.subarray(i, i + CHUNK));
        return btoa(s);
    }
    function fromB64(s) { return Uint8Array.from(atob(s), c => c.charCodeAt(0)); }

    async function deriveKek(secret, salt) {
        const km = await crypto.subtle.importKey("raw", te.encode(secret), "PBKDF2", false, ["deriveKey"]);
        return crypto.subtle.deriveKey(
            { name: "PBKDF2", salt, iterations: ITERATIONS, hash: "SHA-256" },
            km, { name: "AES-GCM", length: 256 }, false, ["encrypt", "decrypt"]);
    }

    async function wrap(rawDek, kek) {
        const iv = crypto.getRandomValues(new Uint8Array(12));
        const ct = await crypto.subtle.encrypt({ name: "AES-GCM", iv }, kek, rawDek);
        const out = new Uint8Array(12 + ct.byteLength);
        out.set(iv); out.set(new Uint8Array(ct), 12);
        return toB64(out.buffer);
    }

    // Throws if the wrapping key is wrong (AES-GCM authenticates).
    async function unwrapToKey(blobB64, kek) {
        const blob = fromB64(blobB64);
        const raw = await crypto.subtle.decrypt({ name: "AES-GCM", iv: blob.slice(0, 12) }, kek, blob.slice(12));
        return crypto.subtle.importKey("raw", raw, "AES-GCM", false, ["encrypt", "decrypt"]);
    }

    // No 0/O/1/I/L/U — unambiguous to read back from paper.
    const CODE_ALPHABET = "ABCDEFGHJKMNPQRSTVWXYZ23456789";
    function makeRecoveryCode() {
        const r = crypto.getRandomValues(new Uint8Array(16));
        let code = "";
        for (let i = 0; i < 16; i++) {
            code += CODE_ALPHABET[r[i] % CODE_ALPHABET.length];
            if (i % 4 === 3 && i < 15) code += "-";
        }
        return code;
    }
    function normalizeCode(c) { return c.toUpperCase().replace(/[^A-Z0-9]/g, ""); }

    return {
        isUnlocked: () => dek !== null,
        lock: () => { dek = null; },

        // First-time setup: mint a DEK, wrap it twice (passphrase + recovery code).
        setup: async (passphrase) => {
            const rawDek = crypto.getRandomValues(new Uint8Array(32));
            const kekSalt = crypto.getRandomValues(new Uint8Array(16));
            const recoverySalt = crypto.getRandomValues(new Uint8Array(16));
            const recoveryCode = makeRecoveryCode();

            const kek = await deriveKek(passphrase, kekSalt);
            const rk = await deriveKek(normalizeCode(recoveryCode), recoverySalt);
            const wrappedDek = await wrap(rawDek, kek);
            const recoveryWrappedDek = await wrap(rawDek, rk);

            dek = await crypto.subtle.importKey("raw", rawDek, "AES-GCM", false, ["encrypt", "decrypt"]);
            rawDek.fill(0);

            return {
                kekSalt: toB64(kekSalt.buffer),
                wrappedDek,
                recoverySalt: toB64(recoverySalt.buffer),
                recoveryWrappedDek,
                recoveryCode
            };
        },

        unlock: async (passphrase, kekSaltB64, wrappedDekB64) => {
            try {
                const kek = await deriveKek(passphrase, fromB64(kekSaltB64));
                dek = await unwrapToKey(wrappedDekB64, kek);
                return true;
            } catch { return false; }
        },

        unlockWithRecovery: async (code, recoverySaltB64, recoveryWrappedDekB64) => {
            try {
                const rk = await deriveKek(normalizeCode(code), fromB64(recoverySaltB64));
                dek = await unwrapToKey(recoveryWrappedDekB64, rk);
                return true;
            } catch { return false; }
        },

        encryptText: async (text) => {
            if (!dek) throw new Error("locked");
            const iv = crypto.getRandomValues(new Uint8Array(12));
            const ct = await crypto.subtle.encrypt({ name: "AES-GCM", iv }, dek, te.encode(text));
            const out = new Uint8Array(12 + ct.byteLength);
            out.set(iv); out.set(new Uint8Array(ct), 12);
            return toB64(out.buffer);
        },

        decryptText: async (blobB64) => {
            if (!dek) throw new Error("locked");
            const blob = fromB64(blobB64);
            const pt = await crypto.subtle.decrypt({ name: "AES-GCM", iv: blob.slice(0, 12) }, dek, blob.slice(12));
            return td.decode(pt);
        }
    };
})();

window.dwtdgUtil = {
    clearHash: () => history.replaceState(null, "", location.pathname + location.search),
    copy: (text) => navigator.clipboard.writeText(text)
};
