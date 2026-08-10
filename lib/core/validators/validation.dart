class ValidationResult {
  final String? fieldError;
  final String? passwordError;

  ValidationResult({this.fieldError, this.passwordError});
}

class Validator {
  static bool isValidEmail(String email) {
    return RegExp(r"^[a-zA-Z0-9.a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9]+\.[a-zA-Z]+")
        .hasMatch(email);
  }

  static ValidationResult validateLogin({
    required bool isEmail,
    required String fieldValue,
    required String passwordValue,
  }) {
    String? fieldError;
    String? passwordError;

    if (isEmail) {
      if (fieldValue.trim().isEmpty) {
        fieldError = "Username or Email required";
      }

      if (passwordValue.isEmpty) {
        passwordError = "Password is required";
      } else if (passwordValue.length < 7) {
        passwordError = "Minimum 7 characters required";
      }
    } else {
      if (fieldValue.length != 10) {
        fieldError = "Enter valid 10 digit number";
      }

      if (passwordValue.isEmpty) {
        passwordError = "OTP required";
      }
    }

    return ValidationResult(
      fieldError: fieldError,
      passwordError: passwordError,
    );
  }
}
