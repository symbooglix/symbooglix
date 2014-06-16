// RUN: %symbooglix %s 2>&1 | %OutputCheck %s
var g:bv8;

procedure main()
// CHECK-L: Concretising  g := 0bv8
requires g == 0bv8;
modifies g;
// CHECK-L: Mutating tree: '2bv8 == 1bv8' => 'false'
// CHECK-L: State 0 terminated with an error
// CHECK-L: The following ensures failed
// CHECK-L: ${CHECKFILE_NAME}:${LINE:+1}: g == 1bv8
ensures g == 1bv8;
{
    g := 2bv8;
}
