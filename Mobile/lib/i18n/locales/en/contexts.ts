import type { contexts as trContexts } from '../tr/contexts';
import type { Translation } from '../types';

export const contexts: Translation<typeof trContexts> = {
  is: {
    name: 'Work & Career',
    description: 'Bosses, overtime, client pressure',
  },
  aile: {
    name: 'Family & Relatives',
    description: 'Parents, in-laws and their favours',
  },
  arkadas: {
    name: 'Friends & Social',
    description: 'Invitations, groups, peer pressure',
  },
  romantik: {
    name: 'Romantic Relationships',
    description: 'Partners and exes',
  },
  satici: {
    name: 'Sales & Services',
    description: 'Shops, banks, tradespeople, phone offers',
  },
  komsu: {
    name: 'Home & Neighbours',
    description: 'Flatmates, neighbours, landlords',
  },
  okul: {
    name: 'School & Campus',
    description: 'Group projects, lecturers, classmates',
  },
  dijital: {
    name: 'Digital & Social Media',
    description: 'Group chats, messages, pressure to post',
  },
};
