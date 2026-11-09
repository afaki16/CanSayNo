import type { common as trCommon } from '../tr/common';
import type { Translation } from '../types';

export const common: Translation<typeof trCommon> = {
  ok: 'OK',
  cancel: 'Cancel',
  save: 'Save',
  retry: 'Try again',
  back: 'Back',
  continue: 'Continue',
  loading: 'Loading',
  errors: {
    network: "Couldn't connect. Check your internet connection",
    session: 'Your session has expired. Please sign in again',
    generic: 'Something went wrong. Please try again',
    tooFast: "You're going a bit fast. Wait a few seconds and try again.",
  },
  language: {
    title: 'Language',
    subtitle: 'Language of the app and scenarios',
    tr: 'Türkçe',
    en: 'English',
    note: 'Scenarios appear in the language you choose. Your past practices stay as they are.',
  },
};
