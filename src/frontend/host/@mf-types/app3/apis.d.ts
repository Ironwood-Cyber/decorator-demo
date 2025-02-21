
    export type RemoteKeys = 'app3/Form';
    type PackageType<T> = T extends 'app3/Form' ? typeof import('app3/Form') :any;