import _ from 'lodash';
import moment from 'moment';

export const USDollar = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD'
});

export const Capitalize = (word) => {
  return word.charAt(0).toUpperCase() + word.slice(1);
};

export const FormatDate = (dateString) => {
  const formats = [
    "YYYY-MM-DDTHH:mm:ssZ", // First format to try
    "MM/DD/YYYY h:mm:ss A" // Second format to try
  ];

  for (const format of formats) {
    const momentDate = moment.utc(dateString, format);
    if (momentDate.isValid()) {
      return momentDate.format('MMMM DD, YYYY hh:mm a');
    }
  }

  return 'Invalid date'; // Return default value if parsing fails for all formats
};

export const DiffObjects = (obj1, obj2) => {
  const diff = _.differenceWith(_.toPairs(obj1), _.toPairs(obj2), _.isEqual);
  return diff
    .map(([key, value]) => ({
      [key]: value
    }))
    .reduce((r, c) => Object.assign(r, c), {});
};
