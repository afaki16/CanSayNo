// react-hook-form ile Input arasındaki boilerplate'i azaltan wrapper.
// Controller'ı sarar; value/onChange/onBlur bağlar ve fieldState.error'ı
// otomatik Input'un `error` prop'una geçirir.
//
// Kullanım:
//   <FormField name="email" control={control} label="E-posta" leftIcon={...} />

import {
  Controller,
  type Control,
  type FieldValues,
  type Path,
} from 'react-hook-form';

import { Input, type InputProps } from './Input';

// `name` ve `control` form'a özgü; geri kalan her şey Input'a forward edilir.
// `value`/`onChange`/`onBlur`/`error` Controller tarafından yönetildiği için dışlanır.
export interface FormFieldProps<T extends FieldValues>
  extends Omit<InputProps, 'value' | 'onChangeText' | 'onBlur' | 'error'> {
  name: Path<T>;
  control: Control<T>;
}

export function FormField<T extends FieldValues>({
  name,
  control,
  ...inputProps
}: FormFieldProps<T>) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field: { value, onChange, onBlur }, fieldState }) => (
        <Input
          {...inputProps}
          value={value}
          onChangeText={onChange}
          onBlur={onBlur}
          error={fieldState.error?.message}
        />
      )}
    />
  );
}
