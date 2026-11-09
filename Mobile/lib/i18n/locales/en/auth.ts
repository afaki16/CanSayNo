import type { auth as trAuth } from '../tr/auth';
import type { Translation } from '../types';

export const auth: Translation<typeof trAuth> = {
  welcome: {
    title: 'Saying no\nis a skill.',
    subtitle: 'Practise in safe scenarios, then hold your boundaries with ease in real life.',
    start: "Let's get started",
    haveAccount: 'I already have an account',
  },
  fields: {
    email: 'Email',
    emailPlaceholder: 'you@example.com',
    password: 'Password',
    passwordPlaceholder: 'Enter your password',
    newPassword: 'New password',
    newPasswordPlaceholder: 'At least 8 characters',
    firstName: 'First name',
    firstNamePlaceholder: 'Your first name',
    lastName: 'Last name',
    lastNamePlaceholder: 'Your last name',
    code: 'Verification code',
    showPassword: 'Show password',
    hidePassword: 'Hide password',
  },
  validation: {
    email: 'Enter a valid email address',
    passwordMin6: 'Password must be at least 6 characters',
    passwordMin8: 'Password must be at least 8 characters',
    passwordUppercase: 'Must include at least one capital letter',
    passwordDigit: 'Must include at least one number',
    firstNameMin: 'First name must be at least 2 characters',
    lastNameMin: 'Last name must be at least 2 characters',
    codeLength: 'The code must be 6 digits',
  },
  login: {
    title: 'Welcome back 👋',
    subtitle: 'Pick up where you left off.',
    forgotPassword: 'Forgot password?',
    submit: 'Sign in',
    noAccount: "Don't have an account?",
    signUp: 'Sign up',
  },
  register: {
    title: 'Create your account',
    subtitle: 'Say your first “no” today.',
    submit: 'Create account',
    terms:
      'By continuing, you agree to the <terms>Terms of Use</terms> and <privacy>Privacy Policy</privacy>.',
    haveAccount: 'Already have an account?',
    signIn: 'Sign in',
  },
  forgotPassword: {
    title: 'Forgotten your password?',
    subtitle: "Enter your email and we'll send you a 6-digit verification code.",
    submit: 'Send code',
  },
  resetPassword: {
    title: 'Enter the code',
    subtitleWithEmail:
      "If <bold>{{email}}</bold> is registered, we've sent it a 6-digit code. It's valid for 10 minutes.",
    subtitle: "We've sent a 6-digit code to your email. It's valid for 10 minutes.",
    submit: 'Update password',
    resendFailedTitle: "Couldn't send the code",
    resendFailedMessage: 'Please try again in a little while.',
    successTitle: 'Password updated',
    successMessage: 'You can now sign in with your new password.',
    successAction: 'Sign in',
    noCode: "Didn't get a code?",
    resend: 'Resend',
    resendCountdown: 'Resend ({{seconds}}s)',
  },
  verifyEmail: {
    title: 'Check your email',
    descriptionWithEmail:
      "We've sent a verification link to <bold>{{email}}</bold>. Once you've tapped it, you can sign in.",
    description:
      "We've sent a verification link to your email. Once you've tapped it, you can sign in.",
    hint: "Can't see it in your inbox? Don't forget to check your spam folder.",
    submit: 'Sign in',
  },
  errors: {
    invalidCredentials: 'Incorrect email or password',
    emailNotConfirmed: 'You need to verify your email. Check your inbox.',
    tooManyAttempts: 'Too many attempts. Wait a moment and try again',
    emailInUse: 'This email address is already used by another account',
    emailAlreadyRegistered: 'This email address is already registered',
    profileSaveFailed: "Couldn't save your details. Please try again",
  },
  api: {
    sessionExpired: 'Your session has expired. Please sign in again',
    connectionFailed: "Couldn't connect",
    requestFailed: 'The request failed',
    unexpectedResponse: "The server didn't return the expected data",
  },
};
