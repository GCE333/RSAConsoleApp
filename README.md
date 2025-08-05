# RSA Console App

![.NET Build Status](https://github.com/GCE333/RSAConsoleApp/actions/workflows/dotnet.yml/badge.svg)

A straightforward command-line application designed to help students understand and experiment with the RSA cryptographic algorithm. This tool allows for key generation, encryption, and decryption, demonstrating core RSA principles and the importance of padding.

**Please Note:** This software is created for educational purposes to demonstrate RSA concepts. While it implements standard algorithms, it is **not intended for use in production environments** or for securing sensitive data.

## Table of Contents

- [Features](#features)
- [How RSA Works Here](#how-rsa-works-here)
- [Getting Started](#getting-started)
    - [Pre-built Binaries](#pre-built-binaries)
    - [Building from Source](#building-from-source)
- [Usage](#usage)
- [Under the Hood](#under-the-hood)
- [License](#license)
- [Acknowledgements](#acknowledgements)

## Features

*   **RSA Key Generation:** Generate new RSA public and private key pairs of a specified bit length (e.g., 512, 1024, 2048 bits).
*   **RSA Key Calculation:** Calculate `n`, `phi(n)`, and the private exponent `d` from chosen prime numbers `p` and `q`, and a public exponent `e`.
*   **RSA Key Import:** Load existing RSA key components (modulus `n`, public exponent `e`, and optionally private exponent `d`) from various formats.
*   **Encryption and Decryption:** Encrypt and decrypt messages using the generated, calculated, or imported RSA keys.
*   **Padding Support:** Choose to use or skip PKCS #1 v1.5 padding during encryption and decryption. **It is highly recommended to use padding for security, even in this educational context, to understand its role.**
*   **Flexible Input/Output Formats:** Input and output messages/ciphertexts in Base64, Hexadecimal, Text (UTF-8), or raw Number formats.
*   **Interactive Console Interface:** User-friendly prompts guide you through the process.
*   **Cross-Platform:** Available as self-contained executables for Windows, Linux, and macOS.

## How RSA Works Here

At its core, RSA relies on the difficulty of factoring large numbers. This application implements the fundamental steps:

1.  **Key Generation:**
    *   Two large prime numbers, `p` and `q`, are chosen (verified using the Miller-Rabin primality test).
    *   The modulus `n` is calculated as `n = p * q`.
    *   Euler's totient function `phi(n)` is calculated as `phi(n) = (p - 1) * (q - 1)`.
    *   A public exponent `e` (commonly 65537) is chosen such that `1 < e < phi(n)` and `e` is coprime with `phi(n)` (i.e., `GCD(e, phi(n)) = 1`).
    *   The private exponent `d` is calculated as the modular multiplicative inverse of `e` modulo `phi(n)` (i.e., `(d * e) mod phi(n) = 1`).
    *   The **public key** consists of `(n, e)`.
    *   The **private key** consists of `(n, d)`. (Note: `p` and `q` are also part of the private key for some implementations, but not directly used in the encryption/decryption primitives here).

2.  **Encryption (Public Key):**
    *   A message `M` is converted into a numerical representation `m`.
    *   The ciphertext `C` is computed as `C = m^e mod n`.

3.  **Decryption (Private Key):**
    *   The ciphertext `C` is converted into a numerical representation `c`.
    *   The original message `M` is recovered by computing `M = c^d mod n`.

### The Importance of Padding

Without padding, RSA is vulnerable to several attacks (e.g., chosen-ciphertext attacks, small message attacks). **In cryptography, padding refers to the practice of adding extra data to a message before encryption.** This additional data typically includes random bytes and specific structural elements. This application implements **PKCS #1 v1.5 padding**, which transforms the message into an `Encoded Message (EM)` by adding random data and specific formatting. This `EM` is then encrypted. During decryption, the `EM` is recovered, and the padding is verified and removed to extract the original message. Using padding is crucial for the security of RSA in real-world applications.

## Getting Started

### Pre-built Binaries

The easiest way to get started is to download the pre-built, self-contained executables from the [Releases page](https://github.com/GCE333/RSAConsoleApp/releases).
Look for the following files:

*   **Windows:** `RSAConsoleApp-win-x64.exe`
*   **Linux:** `RSAConsoleApp-linux-x64`
*   **macOS:** `RSAConsoleApp-osx-x64`

Simply download the appropriate file for your operating system and run it directly.

### Building from Source

To build the application yourself, you will need the .NET 6.0 SDK or newer.

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/GCE333/RSAConsoleApp.git
    cd RSAConsoleApp # Navigate into the project directory
    ```
2.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```
3.  **Build the project:**
    ```bash
    dotnet build
    ```
4.  **Run the application:**
    ```bash
    dotnet run
    ```
    Alternatively, you can publish a self-contained executable for your platform:
    ```bash
    # For Linux x64:
    dotnet publish -c Release --self-contained true -r linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true

    # For Windows x64:
    dotnet publish -c Release --self-contained true -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true

    # For macOS x64:
    dotnet publish -c Release --self-contained true -r osx-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true
    ```

## Usage

Run the program:

*   **Windows:** Double-click the executable file (e.g., `RSAConsoleApp-win-x64.exe` or `RSAConsoleApp.exe`).
*   **Linux/macOS:**
    1.  Open your terminal.
    2.  Navigate to the directory where you saved the executable (e.g., `RSAConsoleApp-linux-x64` or `RSAConsoleApp-osx-x64`, or `RSAConsoleApp` if built locally).
    3.  Grant execute permission: `chmod +x <Executable_Name>` (e.g., `chmod +x RSAConsoleApp-linux-x64` or `chmod +x RSAConsoleApp`).
    4.  Run the application: `./<Executable_Name>` (e.g., `./RSAConsoleApp-linux-x64` or `./RSAConsoleApp`).

The program will present an interactive menu:

1.  **Encrypt or Decrypt?**
    *   Choose `E` for encryption or `D` for decryption.
2.  **Use Padding?**
    *   Choose `Y` to use PKCS #1 v1.5 padding (recommended) or `N` to encrypt/decrypt without padding (less secure, primarily for educational demonstration of the raw RSA primitive).
3.  **Key Management:**
    *   **`G` (Generate):** Generates new RSA keys. You'll be prompted for the key size (number of bits for `n`). For learning, start with smaller sizes (e.g., 512 bits) before trying 1024 or 2048.
    *   **`I` (Import):** Allows you to input `n`, `e`, and `d` (if decrypting) from pre-existing values in Hex or Number format.
    *   **`C` (Calculate):** Allows you to input `p`, `q`, and `e` from which the program will calculate `n`, `phi(n)`, and `d`. This is great for understanding how key components relate.
    *   **`E` (Existing):** If keys were already generated/imported/calculated in the current session, you can reuse them.
4.  **Input/Output Formats:**
    *   When entering a message/ciphertext, you can specify its format (Base64, Hex, Text, Number).
    *   When displaying the result, you can choose the desired output format.

### Example Walkthrough (Generating Keys, Encrypting, Decrypting with Padding)

```
Do you want to encrypt or decrypt? [E]ncrypt/[D]ecrypt/[C]ancel: E
Do you want to use padding? If yes, then PKCS #1 v1.5 padding will be used [Y]es/[N]o: Y
Do you want to generate, import, or calculate the values of new RSA keys? [G]enerate/[I]mport/[C]alculate: G
Enter key size (number of bits in the RSA modulus n): 512
p: [large prime number p]
q: [large prime number q]
n: [modulus n]
phiN: [phi(n)]
e: 65537
d: [private exponent d]
RSA keys generated in XXX.XX ms
Choose the format of your message that you want to encrypt. [B]ase64/[H]ex/[T]ext/[N]umber: T
Enter your message: Hello RSA!
Encrypted in YYY.YY ms
Choose output format of the ciphertext. [B]ase64/[H]ex/[N]umber: H
Ciphertext: [hexadecimal ciphertext]

Do you want to encrypt or decrypt? [E]ncrypt/[D]ecrypt/[C]ancel: D
Do you want to use padding? If yes, then PKCS #1 v1.5 padding will be used [Y]es/[N]o: Y
Do you want to generate, import, or calculate the values of new RSA keys or use existing keys? [G]enerate/[I]mport/[C]alculate/[E]xisting: E
Choose the format of your ciphertext that you want to decrypt. [B]ase64/[H]ex/[N]umber: H
Enter your ciphertext: [paste hexadecimal ciphertext from above]
Decrypted in ZZZ.ZZ ms
Choose output format of the decrypted message. [B]ase64/[H]ex/[T]ext/[N]umber: T
Decrypted message: Hello RSA!
```

## Under the Hood

This project leverages the following concepts and algorithms:

*   **`System.Numerics.BigInteger`**: Essential for handling the extremely large numbers (hundreds or thousands of bits) required for RSA calculations, which standard `int` or `long` types cannot accommodate.
*   **Miller-Rabin Primality Test**: Used in `RSAKeyGenerator` to efficiently determine if a large number is "probably prime" with a very high degree of certainty. This probabilistic test is a cornerstone for finding the prime factors `p` and `q`.
*   **Extended Euclidean Algorithm**: Implicitly used within the `ModularInverse` method in `RSAKeyGenerator` to calculate the private exponent `d`.
*   **Modular Exponentiation**: Implemented using `BigInteger.ModPow` for the core `m^e mod n` (encryption) and `c^d mod n` (decryption) operations.
*   **PKCS #1 v1.5 Padding**: As discussed, this crucial component is implemented in `RSACryptography` to ensure the security and robustness of the RSA operations, adhering closely to [RFC 3447, Section 7.2](https://www.rfc-editor.org/rfc/rfc3447#section-7.2).

## License

This project is licensed under the MIT License. See the [LICENSE.txt](LICENSE.txt) file for details.

## Acknowledgements

*   Developed by George Economakis.
*   Built with the .NET platform.
*   Inspired by the cryptographic principles outlined in various academic resources and RFCs.
